using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Special/PlayerMovement")]
    public class PlayerMovementAbilitySO : UnitAbilitySO
    {
        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var movesLeft = caster.MovementLeft;
            var startTile = caster.Tile;

            if (movesLeft <= 0) return false;
            if (startTile == selectableTile) return false;

            return startTile.IsInAdjacentTileDistance(selectableTile,movesLeft,caster.Stats.WalkableTileSelector);
        }
        
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            var destination = targetTiles[0];
            
            var path = new List<Tile>() {targetTiles[0]};
            var lastAdded = destination;

            for (int i = destination.PathRing - 1; i >= 1; i--)
            {
                lastAdded = lastAdded.GetAdjacentTiles().FirstOrDefault(tile => tile.PathRing == i);
                if (lastAdded == null) break;
                path.Add(lastAdded);
            }

            path.Reverse();
            
            caster.MoveUnit(path,EndAbility);
        }
    }
}