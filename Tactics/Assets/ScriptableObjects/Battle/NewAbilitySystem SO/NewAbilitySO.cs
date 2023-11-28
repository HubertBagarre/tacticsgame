using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/New Ability")]
    public class NewAbilitySO : ScriptableObject
    {
        
        [SerializeField,TextArea(1,10)] private string requirementsParameters;
        
        [SerializeField] private List<AbilityConditionSO> requirements = new ();

        public bool MatchesRequirements(NewTile casterTile)
        {
            if(requirements.Count <= 0) return true;

            return requirements.All(requirement => requirement.CheckTileFullParameters(casterTile, casterTile,GetDictionary(requirementsParameters)));
        }
        
        public string RequirementsText(NewTile casterTile)
        {
            if(requirements.Count <= 0) return string.Empty;
            
            var text = "Requires %COUNT% %TARGET%";
            var count = 1;
            
            (string targetText, string countText) texts = (string.Empty,$"{count}");
            
            foreach (var requirement in requirements)
            {
                text += requirement.TextFullParameters(casterTile,GetDictionary(requirementsParameters));
                var req = requirement.TargetOverrideFullParameters(casterTile, count, GetDictionary(requirementsParameters));

                if (req.targetText != string.Empty && texts.targetText == string.Empty) texts = req;
            }

            if (texts.targetText == string.Empty) texts.targetText = "tile";
            
            text = text.Replace("%TARGET%",texts.targetText);
            text = text.Replace("%COUNT%",texts.countText);
            
            return text;
        }
        
        private static ReadOnlyDictionary<string,string> GetDictionary(string parameters)
        {
            var dict = new Dictionary<string,string>();
            var parametersArray = parameters.Split('\n');

            if(parametersArray.Length <= 0) return new ReadOnlyDictionary<string, string>(dict);

            foreach (var parameter in parametersArray)
            {
                var split = parameter.Split(':');
                
                dict.Add(split[0],split.Length > 1 ? split[1] : string.Empty);
            }
            
            return new ReadOnlyDictionary<string, string>(dict);
        }

        [ContextMenu("Test Requirement Text")]
        private void TestRequirementText()
        {
            Debug.Log(RequirementsText(null));
        }
    }
}


