using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    public abstract class UnitAbilitySelectorSO : ScriptableObject
    {
        [field: SerializeField] public int ExpectedSelections { get; private set; } = 1;
        
        public virtual bool ConvertDescriptionLinks(Unit caster,string linkKey,out string text)
        {
            text = string.Empty;
            return false;
        }

        public abstract string ConvertedDescription(Unit caster);
        
        public bool IsTileSelectable(Unit caster, Tile tile, List<Tile> currentlySelectedTiles)
        {
            return TileSelectionMethod(caster, tile, currentlySelectedTiles);
        }

        protected abstract bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles);
        
        public virtual List<Tile> GetAffectedTiles(Unit caster,Tile lastSelected, List<Tile> selectedTiles)
        {
            return new List<Tile>{lastSelected};
        }

    }
}