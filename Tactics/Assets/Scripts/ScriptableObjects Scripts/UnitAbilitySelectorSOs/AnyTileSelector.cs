using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/Any Tile")]
    public class AnyTileSelector : UnitAbilitySelectorSO
    {
        public override string ConvertedDescription(Unit caster)
        {
            return "Select any tile.";
        }

        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return true;
        }
    }
}
