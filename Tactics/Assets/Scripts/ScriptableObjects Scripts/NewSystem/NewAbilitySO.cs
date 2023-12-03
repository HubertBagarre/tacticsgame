using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battle.ScriptableObjects
{
    using Ability;
    using Ability.Components;
    
    public interface IIdHandler
    {
        string Id { get; }
    }
    
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
        
        [FormerlySerializedAs("selectedEffects")]
        [field: Space]
        [Header("Lua")]
        [SerializeField,TextArea(2,20)] private string parameters;        
        [SerializeField] private List<ConditionalEffect<EffectSO>> availableSelectionConditionalEffects = new ();
        [SerializeField] private List<CustomizableCondition<AbilityConditionSO>> availableConditions = new ();
        [SerializeField] private List<EffectsOnTarget<EffectSO>> availableEffects = new ();
        [SerializeField] private List<PassiveSO> availablePassives = new ();
        
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
            //TODO - lua string conversion goes here
            var text = $"{parameters}";
            
            return text;
        }

        public string FullDescription(NewUnit caster)
        {
            var text = string.Empty;

            text += SelectedEffectsText(caster);

            return text;
        }
        
        public List<EffectsOnTarget<EffectSO>> GetConditionalEffects(NewUnit caster, [CanBeNull] NewTile[] targetTiles,string luaScript)
        {
            var abilityParameterInterpreter = new AbilityParameterInterpreter(caster,targetTiles,availableSelectionConditionalEffects,availableConditions,availableEffects);
            
            var scriptCode = luaScript.Replace("%PARAMETERS%", parameters);
            
            var script = new Script
            {
                Globals =
                {
                    ["injector"] = abilityParameterInterpreter
                }
            };
            
            try
            {
                script.DoString(scriptCode);
                
                return abilityParameterInterpreter.SelectedEffects;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Lua error: " + e.Message);
            }
            
            return new List<EffectsOnTarget<EffectSO>>();
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


