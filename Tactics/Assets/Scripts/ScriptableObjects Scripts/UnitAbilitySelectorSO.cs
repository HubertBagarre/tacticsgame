using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    public abstract class UnitAbilitySelectorSO : ScriptableObject
    {
        [SerializeField,Min(0)] private int expectedSelections = 1;
        public int ExpectedSelections => OverrideExpectedSelections() >= 0 ? OverrideExpectedSelections() : expectedSelections;
        [field: SerializeField] public bool UseSelectionOrder { get; private set; } = true;

        protected virtual int OverrideExpectedSelections()
        {
            return -1;
        }
        
        public virtual bool ConvertDescriptionLinks(Unit caster,string linkKey,out string text)
        {
            text = string.Empty;
            return false;
        }

        public abstract string SelectionDescription(Unit caster);

        public virtual string AffectedDescription(Unit caster)
        {
            return string.Empty;
        }

        public virtual void ChangeAppearanceForTileSelectionStart(Unit caster){}
        public virtual void ChangeAppearanceForTileSelectionChanged(Unit caster,List<Tile> currentlySelectedTiles){}
        
        public bool IsTileSelectable(Unit caster, Tile tile, List<Tile> currentlySelectedTiles)
        {
            return TileSelectionMethod(caster, tile, currentlySelectedTiles);
        }

        public abstract bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="lastSelected"></param>
        /// <param name="selectedTiles"> includes lastSelected</param>
        /// <returns></returns>
        public virtual List<Tile> GetAffectedTiles(Unit caster,Tile lastSelected, List<Tile> selectedTiles)
        {
            return new List<Tile>{lastSelected};
        }

    }
}