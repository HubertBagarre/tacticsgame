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
            caster.Tile.ClearPath();
            
            var startTile = caster.Tile;
            if (startTile == selectableTile) return false;

            var movesLeft = caster.MovementLeft;
            if (movesLeft <= 0) return false;

            var func = caster.Behaviour.WalkableTileSelector;
            
            return startTile.GetAdjacentTiles(movesLeft,func).Contains(selectableTile);
        }

        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            var tile = caster.Tile;
            tile.ClearPath();
            if (tile.GetPath(lastSelected, out var path))
            {
                tile.SetPath(path);
                tile.ShowPath();
                tile.ShowBorder(path);
            }
            
            return base.GetAffectedTiles(caster, lastSelected, selectedTiles);
        }
    }
}