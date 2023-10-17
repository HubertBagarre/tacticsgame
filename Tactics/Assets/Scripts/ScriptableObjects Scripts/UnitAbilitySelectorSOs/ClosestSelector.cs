using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/Closest")]
    public class ClosestSelector : UnitAbilitySelectorSO
    {
        [SerializeField] private UnitAbilitySelectorSO selector;
        [SerializeField,Tooltip("-1 is Infinite")] private int range = 3;
        [SerializeField] private bool useAdjacentTiles = false;
        [SerializeField,Min(1)] private int targetUnits = 3;
        [SerializeField] private bool targetAllies = false;

        protected override int OverrideExpectedSelections()
        {
            return selector == null ? 1 : selector.ExpectedSelections;
        }
        
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("ring:"))
            {
                if(selector != null) return selector.ConvertDescriptionLinks(caster, linkKey, out text);
                return false;
            }
            
            var split = linkKey.Split(":");
            
            if(!int.TryParse(split[1],out var ring))
            {
                if(selector != null) return selector.ConvertDescriptionLinks(caster, linkKey, out text);
                return false;
            }

            var tiles = 0;
            for (int i = 0; i < ring+1; i++)
            {
                tiles += 4 * (2*i-1) + 4;
            }
            
            text = $"The {tiles} tiles surrounding the caster's tile"; //TODO - make generic ring shower (actually a shape shower)

            return true;
        }

        public override string SelectionDescription(Unit caster)
        {
            return selector == null ? string.Empty : selector.SelectionDescription(caster);
        }

        public override string AffectedDescription(Unit caster)
        {
            var countText = string.Empty;
            if (targetUnits > 1) countText = $" {targetUnits}";

            var teamText = targetAllies ? "ally" : "enemy";

            var rangeText = string.Empty;
            if (range > 0) rangeText = $" within <u><link=\"ring:{range}\">{range} rings</link></u>"; //TODO - Add support for adjacent method
            
            return $"<color=green> the closest{countText} {teamText} unit{(targetUnits > 1 ? "s":"")}{rangeText}</color>";
        }

        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return selector == null || selector.TileSelectionMethod(caster,selectableTile,currentlySelectedTiles);
        }
        
        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            if (selector == null) lastSelected = caster.Tile;
            var tiles = lastSelected.GetSurroundingTiles(range).Where(tile => tile.HasUnit()).ToList();

            var unitQueue = new Queue<Tile>();
            foreach (var tile in tiles)
            {
                var sameTeam = tile.Unit.Team == caster.Team;
                
                if((sameTeam && targetAllies) || (!sameTeam && !targetAllies)) unitQueue.Enqueue(tile);
            }
            
            var tilesToReturn = new List<Tile>();
            
            while (unitQueue.Count > 0 && tilesToReturn.Count < targetUnits)
            {
                tilesToReturn.Add(unitQueue.Dequeue());
            }
            
            return tilesToReturn;
        }
    }
}