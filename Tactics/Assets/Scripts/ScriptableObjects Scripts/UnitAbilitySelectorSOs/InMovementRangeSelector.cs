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

        public override string Description(Unit caster)
        {
            return "a tile within Movement range";
        }

        public override void ChangeAppearanceForTileSelectionStart(Unit caster)
        {
            var accessible = caster.Tile.GetAdjacentTiles(caster.MovementLeft, caster.Stats.Behaviour.WalkableTileSelector);
            accessible.Add(caster.Tile);
            
            Tile.ShowBorder(accessible);
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            caster.Tile.ClearPath();
            
            var startTile = caster.Tile;
            if (startTile == selectableTile) return false;

            var movesLeft = caster.MovementLeft;
            if (movesLeft <= 0) return false;

            var func = caster.Stats.Behaviour.WalkableTileSelector;
            
            return startTile.GetAdjacentTiles(movesLeft,func).Contains(selectableTile);
        }

        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            var tile = caster.Tile;
            tile.ClearPath();
            
            if (tile.GetPath(lastSelected, out var path,false,caster.Stats.Behaviour.WalkableTileSelector))
            {
                tile.SetLineRendererPath(path);
                tile.ShowLineRendererPath();
            }
            
            return base.GetAffectedTiles(caster, lastSelected, selectedTiles);
        }
    }
}