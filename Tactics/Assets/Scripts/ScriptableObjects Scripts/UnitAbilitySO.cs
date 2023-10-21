using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.AbilityEvents;

using UnityEngine;

namespace Battle.ScriptableObjects.Ability
{
    using Selector;
    using Effect;

    public enum AbilityType
    {
        None,Movement,Offensive,Defensive
    }

    [CreateAssetMenu(menuName = "Battle Scriptables/Ability")]
    public class UnitAbilitySO : ScriptableObject
    {
        [field: Header("Ability Description")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AbilityType Type { get; private set; }
        [SerializeField, TextArea(10, 10)] protected string description;
        [field:SerializeField] public UnitAbilitySelectorSO Selector { get; private set; }
        [field:SerializeField] public UnitAbilityEffectSO[] Effects { get; private set; }
        
        [field:Header("Costs")]
        [field: SerializeField,Min(0)] public int Cooldown { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public bool IsUltimate { get; private set; } = false;
        [field: SerializeField] public int UltimateCost { get; private set; } = 0;
        
        [field: Header("Special")]
        [field: SerializeField] public bool SkipTargetSelection { get; private set; } = false;
        [field: SerializeField] public bool SkipTargetConfirmation { get; private set; } = false;
        [field: SerializeField] public bool EndUnitTurnAfterCast { get; private set; } = true;

        public virtual string ConvertedDescription(Unit caster)
        {
            
            var effectsText = string.Empty;
            foreach (var effect in Effects)
            {
                var desc = effect.ConvertedDescription(caster);
                if(!desc.EndsWith("\n")) desc += "\n";
                effectsText += $"{desc}";
            }
            effectsText = effectsText.TrimEnd('\n');

            if(Selector == null) return $"{effectsText}";
            
            //Maybe do something cleaner of multi effects
            var descriptionText = Selector.SelectionDescription(caster);
            
            var affectedDesc = Selector.AffectedDescription(caster);
            var toAffectedDesc = affectedDesc == string.Empty ? string.Empty : $" to{affectedDesc}";
            effectsText = effectsText.Replace("%toAFFECTED%", $"{toAffectedDesc}");
            effectsText = effectsText.Replace("%AFFECTED%", Selector.AffectedDescription(caster));
            
            return $"Select {descriptionText}.\n{effectsText}";
        }

        public string ConvertDescriptionLinks(Unit caster, string linkKey)
        {
            var text = linkKey;
            if (Selector.ConvertDescriptionLinks(caster,linkKey, out var selectorText)) text = selectorText;
            foreach (var effect in Effects)
            {
                if (effect.ConvertDescriptionLinks(caster,linkKey, out var effectText)) text = effectText;
            }
            return text;
        }
        
        public IEnumerator CastAbility(Unit caster, Tile[] targetTiles)
        {
            foreach (var effect in Effects)
            {
                yield return caster.StartCoroutine(effect.AbilityEffect(caster, targetTiles));
            }
        }
        
        public UnitAbilityInstance CreateInstance()
        {
            return new UnitAbilityInstance(this);
        }
    }
}

namespace Battle
{
    using ScriptableObjects.Ability;
    using ScriptableObjects.Ability.Effect;
    using ScriptableObjects.Ability.Selector;
    
    
    public class UnitAbilityInstance
    {
        public UnitAbilitySO SO { get; }
        private UnitAbilitySelectorSO Selector { get; }
        private UnitAbilityEffectSO[] Effects { get; }
        public int ExpectedSelections => Selector.ExpectedSelections;
        public int SelectionsLeft => Selector.ExpectedSelections - CurrentSelectionCount;
        private int costModifier = 0;
        public int Cost => SO.Cost + costModifier;
        public bool IsUltimate => SO.IsUltimate;
        private int ultimateCostModifier = 0;
        public int UltimateCost => SO.UltimateCost + ultimateCostModifier;
        public int CurrentCooldown { get; private set; }
        public int CurrentSelectionCount => currentSelectedTiles.Count;

        public bool IsTileSelectable(Unit caster, Tile tile)
        {
            if (CurrentSelectionCount >= ExpectedSelections && Selector.UseSelectionOrder) return false;
            
            return Selector.IsTileSelectable(caster, tile, currentSelectedTiles);
        } 

        private List<Tile> currentSelectedTiles = new();
        public Tile[] CurrentSelectedTiles => currentSelectedTiles.ToArray();
        private List<Tile> currentAffectedTiles = new();
        public Tile[] CurrentAffectedTiles => currentAffectedTiles.ToArray();
        private Dictionary<Tile,List<Tile>> affectedTilesDict = new();

        public event Action<int> OnCurrentSelectedTilesUpdated;
        public event Action<int> OnCurrentCooldownUpdated;
        
        public UnitAbilityInstance(UnitAbilitySO unitAbilitySo)
        {
            SO = unitAbilitySo;
            Selector = SO.Selector;
            Effects = SO.Effects;
            
            CurrentCooldown = 0;
            currentAffectedTiles.Clear();
            currentSelectedTiles.Clear();
            affectedTilesDict.Clear();
        }

        public override string ToString()
        {
            return $"{SO.name} (Instance)";
        }

        public void DecreaseCurrentCooldown(int amount)
        {
            CurrentCooldown -= amount;
            if (CurrentCooldown < 0) CurrentCooldown = 0;
        }

        public void IncreaseCurrentCooldown(int amount)
        {
            CurrentCooldown += amount;
        }

        public void EnterCooldown()
        {
            CurrentCooldown = SO.Cooldown;
        }

        public void ClearSelection()
        {
            currentSelectedTiles.Clear();
            currentAffectedTiles.Clear();
            affectedTilesDict.Clear();
        }
        
        public void StartTileSelection(Unit caster)
        {
            Selector.ChangeAppearanceForTileSelectionStart(caster);
        }
        
        //TODO - rework to update affected tiles (also visual), probably add overrides in the so
        public void AddTileToSelection(Unit caster, Tile tile, bool force = false)
        {
            var useSelectionOrder = SO.Selector.UseSelectionOrder;
            
            // remove tile if already selected
            if (currentSelectedTiles.Contains(tile))
            {
                if (!useSelectionOrder)
                {
                    RemoveTileFromSelection(caster, tile);
                    return;
                }

                var index = currentSelectedTiles.IndexOf(tile);
                for (var i = CurrentSelectionCount - 1; i >= index; i--)
                {
                    RemoveTileFromSelection(caster, currentSelectedTiles[i]);
                }
                return;
            }
            
            if (!IsTileSelectable(caster, tile) && !force) return;
            
            // if max selection reached
            if (CurrentSelectionCount >= Selector.ExpectedSelections)
            {
                if(useSelectionOrder) return;
                RemoveTileFromSelection(caster, currentSelectedTiles[^1]);
            }
            
            currentSelectedTiles.Add(tile);
            var affectedTiles = Selector.GetAffectedTiles(caster, tile, currentSelectedTiles);
            
            affectedTilesDict.Add(tile,affectedTiles);
            currentAffectedTiles.AddRange(affectedTiles);
            
            Selector.ChangeAppearanceForTileSelectionChanged(caster,currentSelectedTiles);
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);

            if (SO.SkipTargetConfirmation)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }
        }

        public void RemoveTileFromSelection(Unit caster, Tile tile)
        {
            if (!currentSelectedTiles.Contains(tile)) return;
            currentSelectedTiles.Remove(tile);

            if (affectedTilesDict.TryGetValue(tile, out var tilesToRemove))
            {
                foreach (var affectedTile in tilesToRemove)
                {
                    if (!currentAffectedTiles.Contains(affectedTile)) continue;
                    currentAffectedTiles.Remove(affectedTile);
                }

                affectedTilesDict.Remove(tile);
            }
            
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
        }
        
        public void CastAbility(Unit caster)
        {
            EventManager.Trigger(new StartAbilityCastEvent(this, caster, currentAffectedTiles));

            caster.StartCoroutine(AbilityCast());
            return;

            IEnumerator AbilityCast()
            {
                yield return caster.StartCoroutine(SO.CastAbility(caster, currentAffectedTiles.Distinct().ToArray()));

                OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
                currentSelectedTiles.Clear();
                currentAffectedTiles.Clear();
                
                EventManager.Trigger(new EndAbilityCastEvent(SO,caster));
            }
        }

        public void ResetCost()
        {
            costModifier = 0;
        }

        public void IncreaseCost(int value)
        {
            costModifier += value;
        }
    }
}