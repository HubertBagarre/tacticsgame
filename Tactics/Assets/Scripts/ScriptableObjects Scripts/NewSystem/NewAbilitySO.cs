using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/New Ability")]
    public class NewAbilitySO : ScriptableObject
    {
        [Serializable]
        public struct ConditionalEffects<T> where T : AbilityEffectSO
        {
            [SerializeField] private BranchedConditionalEffect<T>[] conditionalEffects;
            public IReadOnlyCollection<BranchedConditionalEffect<T>> ConditionalEffectsCollection => conditionalEffects;
        }

        [field: SerializeField] public string Name { get; private set; }
        
        [field: Tooltip("Requirements on the caster's tile to be able to cast this ability.")]
        [field: SerializeField] public CustomizableCondition<AbilityConditionSO> Requirements { get; private set; } = new();
        
        [Tooltip("Effects that will be applied to the selected tiles if the ability is cast.")]
        [SerializeField] private ConditionalEffects<AbilityEffectSO> selectedEffects = new ();
        public ConditionalEffects<AbilityEffectSO> SelectedEffects => selectedEffects;
        
        public bool MatchesRequirements(NewUnit caster)
        {
            return Requirements.DoesTileMatchConditionFullParameters(caster.Tile,caster.Tile);
        }
        
        public string RequirementsText(NewTile casterTile)
        {
            var requirements = Requirements.Conditions;
            
            if(requirements.Count <= 0) return string.Empty;
            
            var text = "Requires %COUNT% %TARGET%";
            var count = 1;
            
            (string targetText, string countText) texts = (string.Empty,$"{count}");
            
            foreach (var requirement in requirements)
            {
                text += requirement.TextFullParameters(casterTile,Requirements.Parameters);
                var req = requirement.TargetOverrideFullParameters(casterTile, count, Requirements.Parameters);

                if (req.targetText != string.Empty && texts.targetText == string.Empty) texts = req;
            }

            if (texts.targetText == string.Empty) texts.targetText = "tile";
            
            text = text.Replace("%TARGET%",texts.targetText);
            text = text.Replace("%COUNT%",texts.countText);

            text += ".";
            
            return text;
        }

        [ContextMenu("Test Requirement Text")]
        private void TestRequirementText()
        {
            Debug.Log(RequirementsText(null));
        }
    }
}


