using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/True Selector")]
    public class TrueSelector : UnitAbilitySelectorSO
    {
        public override string Description(Unit caster)
        {
            return string.Empty;
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return true;
        }
    }

}

