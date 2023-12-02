using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using Ability;
    using Ability.Components;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/New Ability")]
    public class NewAbilitySO : ScriptableObject
    {
        [field: Header("Ability Description")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AbilityType Type { get; private set; }
        
        [field:Header("Costs")]
        [field: SerializeField,Min(0)] public int Cooldown { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public bool IsUltimate { get; private set; } = false;
        [field: SerializeField] public int UltimateCost { get; private set; } = 0;
        
        [field: Header("Special")]
        [field: SerializeField] public bool SkipTargetSelection { get; private set; } = false;
        [field: SerializeField] public bool SkipTargetConfirmation { get; private set; } = false;
        [field: SerializeField] public bool EndUnitTurnAfterCast { get; private set; } = true;
        
        [field: Space]
        [field: Tooltip("Requirements on the caster's tile to be able to cast this ability.")]
        [field: SerializeField] public CustomizableAbilityCondition Requirements { get; private set; } = new();

        [field: SerializeField] public int ExpectedSelections { get; private set; } = 1;
        [field: SerializeField] public CustomizableAbilityCondition SelectionCondition { get; private set; } = new();
        
        [field: Space]
        [Tooltip("Effects that will be applied to the selected tiles when the ability is cast.")]
        [SerializeField] private ConditionalEffects<AbilityEffectSO> selectedEffects = new ();
        public ConditionalEffects<AbilityEffectSO> SelectedEffects => selectedEffects;
        
        public bool MatchesRequirements(NewUnit caster)
        {
            return Requirements.DoesTileMatchConditionFullParameters(caster?.Tile,caster?.Tile);
        }

        public bool IsSelectableTile(NewTile referenceTile, NewTile targetTile)
        {
            return SelectionCondition.DoesTileMatchConditionFullParameters(referenceTile, targetTile);
        }
        
        public string RequirementText(NewUnit caster)
        {
            var text = $"Requires {Requirements.ConditionText(caster?.Tile, 1)}.";

            return text;
        }
        
        public string SelectionText(NewUnit caster)
        {
            var selectionConditionText = SelectionCondition.ConditionText(caster?.Tile, ExpectedSelections);
            
            var text = $"Select {selectionConditionText}.";
            if(selectionConditionText == string.Empty) text = "<i>No selection required.</i>";

            return text;
        }
        
        

        public string SelectedEffectsText(NewUnit caster)
        {
            var firstConditionalEffect = SelectedEffects.ConditionalEffectsCollection.First().ConditionalEffect;
            
            return firstConditionalEffect.Text(caster?.Tile);
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


