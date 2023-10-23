using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Selector
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Selector/General Selector")]
    public class GeneralSelector : UnitAbilitySelectorSO
    {
        [SerializeField,Tooltip("-1 is Infinite")] private int range = 2;
        [SerializeField] private bool targetAllies = false;
        [SerializeField] private bool targetEnemies = true;
        private bool TargetUnits => targetAllies || targetEnemies;
        
        [SerializeField] private List<UnitAbilityRequirementSO> requirements = new List<UnitAbilityRequirementSO>();
        
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("ring:")) return ConvertRequirementsDescriptionLinks(out text);
            
            var split = linkKey.Split(":");
            
            if(!int.TryParse(split[1],out var ring)) return ConvertRequirementsDescriptionLinks(out text);
            
            var tiles = 0;
            for (int i = 0; i < ring+1; i++)
            {
                tiles += 4 * (2*i-1) + 4;
            }
            
            text = $"The {tiles} tiles surrounding the caster's tile"; //TODO - make generic ring shower (actually a shape shower)
            return true;
            
            bool ConvertRequirementsDescriptionLinks(out string text)
            {
                text = string.Empty;
                if (requirements.Count <= 0) return false;

                foreach (var requirement in requirements)
                {
                    if(requirement.ConvertDescriptionLinks(caster.Tile,linkKey,out text)) return true;
                }

                return false;
            }
        }

        public override string Description(Unit caster)
        {
            var unitType = string.Empty;
            if (!(targetAllies && targetEnemies))
            {
                if (targetAllies) unitType = "ally ";
                if (targetEnemies) unitType = "enemy ";
            }
            
            var targetType = TargetUnits ? "unit" : "tile";
            
            var targetText = $"<color=green>{ExpectedSelections} {unitType}{targetType}{(ExpectedSelections > 1 ? "s":"")}";
            
            var rangeText = range > 0 ? $" within <u><link=\"ring:{range}\">{range} rings</link></u></color>" : "</color>";
            
            // TODO - manage multiple requirements, add "and" and "," and stuff
            var requirementsText = string.Empty;
            if (requirements.Count > 0)
            {
                requirementsText = " with";
                foreach (var requirement in requirements)
                {
                    foreach (var tuple in requirement.Descriptions(caster.Tile))
                    {
                        requirementsText += $" {tuple.content}";
                        requirementsText += $",";
                    }
                }
                requirementsText = requirementsText.Remove(requirementsText.Length - 1);
            }
            
            return $"{targetText}{rangeText}{requirementsText}";
        }
        
        public override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            if (range >= 0 && !caster.Tile.GetSurroundingTiles(range).Contains(selectableTile)) return false;
            
            var meetRequirements = MeetRequirements(selectableTile);

            if (!TargetUnits) return meetRequirements;
            
            if (!selectableTile.HasUnit()) return false;
            
            var unit = selectableTile.Unit;

            if (targetAllies && unit.Team == caster.Team) return meetRequirements;
            if (targetEnemies && unit.Team != caster.Team) return meetRequirements;
            
            return false;
        }

        private bool MeetRequirements(Tile selectableTile)
        {
            foreach (var requirement in requirements)
            {
                if (!requirement.CanCastAbility(selectableTile)) return false;
            }

            return true;
        }
    }
}


