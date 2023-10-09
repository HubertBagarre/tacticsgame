using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/Area Selector")]
    public class AreaSelectorAbilitySO : UnitAbilitySelectorSO
    {
        //TODO: Add a range to this selector

        public override string SelectionDescription(Unit caster)
        {
            return "a Tile";
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return true;
        }

        public override string AffectedDescription(Unit caster)
        {
            return " to the Surrounding Tiles";
        }

        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            var tiles = lastSelected.GetSurroundingTiles();
            tiles.Add(lastSelected);
            
            return tiles;
        }
    }

}

