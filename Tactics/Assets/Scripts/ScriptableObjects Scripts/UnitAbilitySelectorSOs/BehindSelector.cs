using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Selector
{
    [CreateAssetMenu(fileName = "BehindSelector", menuName = "Battle Scriptables/Ability Selector/Behind Selector")]
    public class BehindSelector : UnitAbilitySelectorSO
    {
        [SerializeField] private UnitAbilitySelectorSO selector;
        
        public override string Description(Unit caster)
        {
            return "a tile behind the target";
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return true;
        }

        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            return new List<Tile>() { lastSelected.GetNeighbor(caster.Tile.GetTileDirection(lastSelected))};
        }
    }
}

