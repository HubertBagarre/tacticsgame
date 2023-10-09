using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/List Selector")]
    public class ListSelector : UnitAbilitySelectorSO
    {
        [SerializeField] private List<UnitAbilitySelectorSO> selectors;

        protected override int OverrideExpectedSelections()
        {
            return selectors.Count;
        }

        public override string SelectionDescription(Unit caster)
        {
            var text = "";
            for (var index = 0; index < selectors.Count; index++)
            {
                var selector = selectors[index];
                var returnChar = (index != 0 ? ", then " : "");
                
                var selectorText = selector.SelectionDescription(caster);
                if(index != 0) selectorText = selectorText.ToLower();
                
                text += $"{returnChar}{selectorText}";
            }

            return text;
        }

        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            foreach (var selector in selectors)
            {
                if(selector.ConvertDescriptionLinks(caster,linkKey,out text)) return true;
            }

            text = linkKey;
            return false;
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var selectedTiles = currentlySelectedTiles.Count;
            if(selectedTiles >= selectors.Count) selectedTiles = selectors.Count - 1;
            return selectors[selectedTiles].TileSelectionMethod(caster,selectableTile,currentlySelectedTiles);
        }
    }
}
