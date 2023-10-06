using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/In Movement Range")]
    public class InMovementRangeSelector : UnitAbilitySelectorSO
    {
        public override string ConvertedDescription(Unit caster)
        {
            return "Select a tile within Movement range.";
        }

        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var startTile = caster.Tile;
            if (startTile == selectableTile) return false;

            var movesLeft = caster.MovementLeft;
            if (movesLeft <= 0) return false;

            return startTile.IsInAdjacentTileDistance(selectableTile,movesLeft,caster.Stats.WalkableTileSelector);
        }
    }
}