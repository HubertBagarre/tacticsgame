using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/In Movement Range")]
    public class InMovementRangeSelector : UnitAbilitySelectorSO
    {
        protected override int OverrideExpectedSelections()
        {
            return 1;
        }

        public override string SelectionDescription(Unit caster)
        {
            return "a tile within Movement range";
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var startTile = caster.Tile;
            if (startTile == selectableTile) return false;

            var movesLeft = caster.MovementLeft;
            if (movesLeft <= 0) return false;

            return startTile.IsInAdjacentTileDistance(selectableTile,movesLeft,caster.Stats.WalkableTileSelector);
        }
    }
}