using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/In Attack Range")]
    public class InAttackRangeSelector : UnitAbilitySelectorSO
    {
        [SerializeField] private bool includeDiag;
        [SerializeField] private bool targetAllies = false;
        [SerializeField] private bool targetEnemies = true;
        private bool TargetUnits => targetAllies || targetEnemies;
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("ring:")) return false;
            
            var split = linkKey.Split(":");
            
            if(!int.TryParse(split[1],out var ring)) return false;
            
            var tiles = 0;
            for (int i = 0; i < ring+1; i++)
            {
                tiles += 4 * (2*i-1) + 4;
            }
            
            text = $"The {tiles} tiles surrounding the caster's tile"; //TODO - make generic ring shower (actually a shape shower)
            return true;
        }

        public override string Description(Unit caster)
        {
            var range = caster.Stats.AttackRange;
            var unitType = string.Empty;
            if (!(targetAllies && targetEnemies))
            {
                if (targetAllies) unitType = "ally ";
                if (targetEnemies) unitType = "enemy ";
            }
            
            var targetType = TargetUnits ? "unit" : "tile";
            
            var targetText = $"<color=green>{ExpectedSelections} {unitType}{targetType}{(ExpectedSelections > 1 ? "s":"")}";
            
            var rangeText = range > 0 ? $" within attack range (<u><link=\"ring:{range}\">{range} rings</link></u>)</color>" : "</color>";
            
            return $"{targetText}{rangeText}";
        }
        
        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            var range = caster.Stats.AttackRange;
            var targetTiles = includeDiag ? caster.Tile.GetSurroundingTiles(range) : caster.Tile.GetAdjacentTiles(range);
            
            if (range >= 0 && !targetTiles.Contains(selectableTile)) return false;

            if (!TargetUnits) return true;
            
            if (!selectableTile.HasUnit()) return false;
            
            var unit = selectableTile.Unit;

            if (targetAllies && unit.Team == caster.Team) return true;
            if (targetEnemies && unit.Team != caster.Team) return true;
            
            return false;
        }
    }
}