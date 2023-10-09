using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/Direction Selector")]
    public class DirectionSelectorSO : UnitAbilitySelectorSO
    {
        [SerializeField] private int expectedDirections = 1;
        [SerializeField] private UnitAbilitySelectorSO startTileSelector;
        [SerializeField] private bool includeDiag = false;

        protected override int OverrideExpectedSelections()
        {
            return startTileSelector != null ? expectedDirections : expectedDirections + 1;
        }

        public override string ConvertedDescription(Unit caster)
        {
            if (startTileSelector != null) return $"{startTileSelector.ConvertedDescription(caster)} Then select a direction.";
            return "Select a direction";
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var startingTile = caster.Tile;
            if (startTileSelector != null)
            {
                if(currentlySelectedTiles.Count == 0) return startTileSelector.IsTileSelectable(caster,selectableTile,currentlySelectedTiles);
                startingTile = currentlySelectedTiles[0];
            }

            /*
            var match = false;
            for (int i = 0; i < 8; i++)
            {
                if(caster.Tile.GetTilesInDirection(i).Contains(selectableTile)) match = true;
            }

            return match;
            */

            return includeDiag
                ? startingTile.IsInSurroundingTileDistance(selectableTile, 1)
                : startingTile.IsInAdjacentTileDistance(selectableTile, 1);

        }

        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            var list = new List<Tile>();
            
            var useSelector = startTileSelector != null;
            var minimumSelection = useSelector ? 1 : 0;
            
            if(selectedTiles.Count < minimumSelection) return list;
            
            var startingTile = caster.Tile;
            if(useSelector) startingTile = selectedTiles[0];
            
            var directionTile = lastSelected;
            
            var direction = startingTile.GetNeighborIndex(directionTile);
            return startingTile.GetTilesInDirection(direction);
        }
    }
}
