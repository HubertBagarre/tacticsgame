using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UIEvents;
    using InputEvent;
    using UnitEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Special/PlayerMovement")]
    public class PlayerMovementAbilitySO : UnitAbilitySO
    {
        protected override bool TileSelectionMethod(Unit caster, Tile tile, List<Tile> currentlySelectedTiles)
        {
            return caster.Tile.IsInAdjacentTileDistance(tile,caster.MovementLeft,caster.Stats.WalkableTileSelector);
        }
        
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            SetSelectableTilesForMovementAndSetInputs(caster);
        }
        
        private void SetSelectableTilesForMovementAndSetInputs(Unit unit)
        {
            var selectableTilesForMovement = new List<Tile>();

            if (!unit.CanMove || unit.MovementLeft <= 0)
            {
                EndAbility();
                return;
            }
            
            SetSelectableTilesForMovement(unit.Tile, unit.MovementLeft, false, unit.Stats.WalkableTileSelector);
            
            if (selectableTilesForMovement.Count <= 0)
            {
                EndAbility();
                return;
            }
            
            EventManager.AddListener<ClickTileEvent>(TryMoveToTile);

            EventManager.Trigger(new StartUnitMovementSelectionEvent(unit, selectableTilesForMovement));
            
            

            void SetSelectableTilesForMovement(Tile origin, int range, bool includeDiag, Func<Tile, bool> extraCondition = null)
            {
                extraCondition ??= _ => true;

                selectableTilesForMovement.Add(origin);
                var justAdded = new List<Tile>() {origin};
                var iteration = 0;

                origin.SetPathRing(iteration);

                AddNeighbors();

                foreach (var tile in selectableTilesForMovement)
                {
                    tile.SetAppearance(Tile.Appearance.Selectable);
                }

                void AddNeighbors()
                {
                    if (iteration >= range) return;

                    var neighbors = justAdded.SelectMany(tile => tile.GetAdjacentTiles()).Distinct()
                        .ToList();

                    var validTiles = neighbors
                        .Where(tile => !selectableTilesForMovement.Contains(tile))
                        .Where(extraCondition)
                        .ToList();

                    selectableTilesForMovement.AddRange(validTiles);
                    justAdded = validTiles;

                    iteration++;

                    foreach (var addedTile in justAdded)
                    {
                        addedTile.SetPathRing(iteration);
                    }

                    AddNeighbors();
                }
            }
            
            void TryMoveToTile(ClickTileEvent clickTileEvent)
            {
                var destination = clickTileEvent.Tile;
                
                if(destination == null) return;
                if(!selectableTilesForMovement.Contains(destination)) return;
                
                EventManager.RemoveListener<ClickTileEvent>(TryMoveToTile);

                if (unit.Tile == destination)
                {
                    Debug.Log("Destination is same as current tile");
                    EndAbility();
                    return;
                }
                
                destination.SetAppearance(Tile.Appearance.Selected);
                
                var path = GetPathFromSelectableTiles(destination);
                
                foreach (var tile in selectableTilesForMovement)
                {
                    tile.SetAppearance(Tile.Appearance.Default);
                    tile.SetPathRing(0);
                }

                EventManager.AddListener<UnitMovementEndEvent>(EndMovementAbility,true);
                
                unit.MoveUnit(path);
                
                void EndMovementAbility(UnitMovementEndEvent movementEndEvent)
                {
                    destination.SetAppearance(Tile.Appearance.Default);
                    
                    EndAbility();
                }
            }
            
            List<Tile> GetPathFromSelectableTiles(Tile destination)
            {
                if (!selectableTilesForMovement.Contains(destination)) return null;

                var path = new List<Tile>() {destination};
                var lastAdded = destination;

                for (int i = destination.PathRing - 1; i >= 1; i--)
                {
                    lastAdded = lastAdded.GetAdjacentTiles().FirstOrDefault(tile => tile.PathRing == i);
                    if (lastAdded == null) break;
                    path.Add(lastAdded);
                }

                path.Reverse();

                return path;
            }
        }
        
    }
}


