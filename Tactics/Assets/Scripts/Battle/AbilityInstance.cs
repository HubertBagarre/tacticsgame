using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.AbilityEvents;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    
    public class AbilityInstance
    {
        private AbilityToAdd Origin { get; }
        public NewAbilitySO SO => Origin.NewAbility;
        public bool ShowInTooltip => Origin.ShowInUI;
        public int ExpectedSelections => SO.ExpectedSelections;
        private int costModifier = 0;
        public int Cost => SO.Cost + costModifier;
        public bool IsUltimate => SO.IsUltimate;
        private int ultimateCostModifier = 0;
        public int UltimateCost => SO.UltimateCost + ultimateCostModifier;
        public int CurrentCooldown { get; private set; }
        public int CurrentSelectionCount => currentSelectedTiles.Count;

        public bool IsTileSelectable(NewUnit caster, NewTile tile)
        {
            return SO.IsSelectableTile(caster.Tile, tile);
        } 

        private List<Tile> currentSelectedTiles = new();
        public Tile[] CurrentSelectedTiles => currentSelectedTiles.ToArray();
        private List<Tile> currentAffectedTiles = new();
        public Tile[] CurrentAffectedTiles => currentAffectedTiles.ToArray();
        private Dictionary<Tile,List<Tile>> affectedTilesDict = new();

        public event Action<int> OnCurrentSelectedTilesUpdated;
        public event Action<int> OnCurrentCooldownUpdated;

        public AbilityInstance(AbilityToAdd origin)
        {
            Origin = origin;
            
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
            
        }
        
        //TODO - rework to update affected tiles (also visual), probably add overrides in the so
        public void AddTileToSelection(Unit caster, Tile tile, bool force = false)
        {
            /*
            var useSelectionOrder = true; SO.Selector.UseSelectionOrder;
            
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
            var affectedTiles = Selector.GetAffectedTiles(caster, tile, currentSelectedTiles).Where(affectedTile => affectedTile != null).Distinct().ToList();

            affectedTilesDict.Add(tile,affectedTiles);
            currentAffectedTiles.AddRange(affectedTiles);
            
            Selector.ChangeAppearanceForTileSelectionChanged(caster,currentSelectedTiles);
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);

            if (SO.SkipTargetConfirmation)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(caster,false));
            }
            */
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
            /*
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
            */
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


