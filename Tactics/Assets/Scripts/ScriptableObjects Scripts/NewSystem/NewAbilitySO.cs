using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/New Ability")]
    public class NewAbilitySO : ScriptableObject
    {
        [Serializable]
        public struct ConditionalEffects<T> where T : AbilityEffectSO
        {
            [SerializeField] private BranchedConditionalEffect<AbilityEffectSO>[] conditionalEffects;
            public IReadOnlyCollection<BranchedConditionalEffect<AbilityEffectSO>> ConditionalEffectsCollection => conditionalEffects;
        }

        [field: Header("Ability Description")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AbilityType Type { get; private set; }
        
        [field: Space]
        [field: Tooltip("Requirements on the caster's tile to be able to cast this ability.")]
        [field: SerializeField] public CustomizableAbilityCondition Requirements { get; private set; } = new();

        [field: SerializeField] public int ExpectedSelections { get; private set; } = 1;
        [field: SerializeField] public CustomizableAbilityCondition SelectionCondition { get; private set; } = new();
        
        [field: Space]
        [Tooltip("Effects that will be applied to the selected tiles if the ability is cast.")]
        [SerializeField] private ConditionalEffects<AbilityEffectSO> selectedEffects = new ();
        public ConditionalEffects<AbilityEffectSO> SelectedEffects => selectedEffects;
        
        public bool MatchesRequirements(NewUnit caster)
        {
            return Requirements.DoesTileMatchConditionFullParameters(caster?.Tile,caster?.Tile);
        }
        
        public string RequirementText(NewUnit caster)
        {
            var text = $"Requires {Requirements.ConditionText(caster?.Tile, 1)}.";

            return text;
        }
        
        public string SelectionText(NewUnit caster)
        {
            var selectionConditionText = SelectionCondition.ConditionText(caster?.Tile, ExpectedSelections);
            
            var text = $"Requires {selectionConditionText}.";
            if(selectionConditionText == string.Empty) text = "<i>No selection required.</i>";

            return text;
        }

        public string SelectedEffectsText(NewUnit caster)
        {
            var firstConditionalEffect = SelectedEffects.ConditionalEffectsCollection.First().ConditionalEffect;
            
            return firstConditionalEffect.EffectsText(caster?.Tile);
        }
        
        [ContextMenu("Test Requirement Text")]
        private void TestRequirementText()
        {
            Debug.Log(RequirementText(null));
        }
        
        [ContextMenu("Test Selection Text")]
        private void TestSelectionText()
        {
            Debug.Log(SelectionText(null));
        }

        [ContextMenu("Test Effect Text")]
        private void TestEffectText()
        {
            Debug.Log(SelectedEffectsText(null));
        }
    }
}


