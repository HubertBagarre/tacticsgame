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

        public bool IsTileSelectable(NewTile tile)
        {
            if(CurrentCaster == null || CurrentSelectionCount >= ExpectedSelections) return false;
            
            return SO.IsSelectableTile(CurrentCaster.Tile, tile);
        } 
        private readonly List<NewTile> currentSelectedTiles = new();
        public IReadOnlyList<NewTile> CurrentSelectedTiles => currentSelectedTiles;
        private readonly List<NewTile> currentAffectedTiles = new();
        public IReadOnlyList<NewTile> CurrentAffectedTiles => currentAffectedTiles;
        private Dictionary<NewTile,List<NewTile>> affectedTilesDict = new();
        
        public NewUnit CurrentCaster { get; private set; }
        public event Action<AbilityInstance> OnCurrentSelectedTilesUpdated;
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
            CurrentCaster = null;
        }
        
        public void StartTileSelection(NewUnit caster)
        {
            ClearSelection();
            
            //TODO add auto selected tiles here
            
            CurrentCaster = caster;
            
            OnCurrentSelectedTilesUpdated?.Invoke(this);
        }
        
        public void EndTileSelection()
        {
            CurrentCaster = null;
            
            OnCurrentSelectedTilesUpdated?.Invoke(this);
        }
        
        //TODO - rework to update affected tiles (also visual), probably add overrides in the so
        public void TryAddTileToSelection(NewTile tile, bool force = false)
        {
            Debug.Log($"Adding {tile} to selection");
            
            if (force)
            {
                AddTileToSelection(tile);
                return;
            }
            
            if (currentSelectedTiles.Contains(tile))
            {
                RemoveTileFromSelection(tile);
                return;
            }
            
            var isTileSelectable = IsTileSelectable(tile);
            
            if(!isTileSelectable) return;
            
            AddTileToSelection(tile);
            
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
        
        private void AddTileToSelection(NewTile tile)
        {
            currentSelectedTiles.Add(tile);
            var affectedTiles = new List<NewTile>() {tile}; // TODO - use affector here

            affectedTilesDict.Add(tile,affectedTiles);
            currentAffectedTiles.AddRange(affectedTiles);
            
            OnCurrentSelectedTilesUpdated?.Invoke(this);
        }

        public void RemoveTileFromSelection(NewTile tile)
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
            
            OnCurrentSelectedTilesUpdated?.Invoke(this);
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


