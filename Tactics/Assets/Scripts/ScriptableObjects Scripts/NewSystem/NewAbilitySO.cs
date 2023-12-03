using System;
using System.Collections.Generic;
using System.Linq;
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
        [Tooltip("Effects that will be applied to the selected tiles when the ability is cast.")]
        [SerializeField] private ConditionalEffects<EffectSO> conditionalEffects = new ();
        public ConditionalEffects<EffectSO> ConditionalEffects => conditionalEffects;
        [Header("Lua")]
        [SerializeField,TextArea(2,20)] private string parameters;        
        [SerializeField,Tooltip("Will be checked for each Affected Tile")] private List<ConditionalEffect<EffectSO>> availableSelectionConditionalEffects = new ();
        [SerializeField,Tooltip("Will be checked on caster")] private List<CustomizableCondition<AbilityConditionSO>> availableConditions = new ();
        [SerializeField] private List<EffectSO> availableEffects = new ();
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
            var firstConditionalEffect = ConditionalEffects.ConditionalEffectsCollection.First().ConditionalEffect;
            
            return firstConditionalEffect.Text(caster?.Tile);
        }
        
        public List<EffectsOnTarget<EffectSO>> GetConditionalEffects(NewUnit caster, NewTile[] targetTiles,string luaScript)
        {
            var effects = new List<EffectsOnTarget<EffectSO>>();
            
            var scriptCode = luaScript.Replace("%PARAMETERS%", parameters);

            var availableSelectionConditionalEffectsConditions =
                availableSelectionConditionalEffects.Select(cEffect => cEffect.Condition).ToList();
            
            var script = new Script
            {
                Globals =
                {
                    ["stringToConditionalIndex"] = GetStringToIndexTable(availableSelectionConditionalEffects),
                    ["stringToConditionIndex"] = GetStringToIndexTable(availableConditions),
                    ["stringToEffectIndex"] = GetStringToIndexTable(availableEffects),
                    
                    ["canCastConditionalTable"] = GetCanCastSelectionTable(availableSelectionConditionalEffectsConditions,targetTiles),
                    ["canCastTable"] = GetCanCastSelectionTable(availableConditions,new []{caster?.Tile})
                }
            };
            
            try
            {
                var res = script.DoString(scriptCode);
                
                var pairs = res.Table.Pairs;
                foreach (var pair in pairs)
                {
                    if(pair.Value.Boolean) AddEffect(pair.Key);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Lua error : " + e.Message);
            }
            
            return effects;

            Dictionary<string, double> GetStringToIndexTable(IReadOnlyList<IIdHandler> listToIndex)
            {
                var stringTable = new Dictionary<string, double>();
                for (var index = 0; index < listToIndex.Count; index++)
                {
                    var conditionalEffect = listToIndex[index];
                    stringTable.Add(conditionalEffect.Id, index);
                }
                
                return stringTable;
            }

            Dictionary<int, bool> GetCanCastSelectionTable(IReadOnlyList<CustomizableCondition<AbilityConditionSO>> conditions,NewTile[] targets)
            {
                var outputTable = new Dictionary<int, bool>();
                for (var index = 0; index < conditions.Count; index++)
                {
                    outputTable.Add(index, CanCastEffect(conditions[index]));
                }

                return outputTable;
                
                bool CanCastEffect(CustomizableCondition<AbilityConditionSO> customizableCondition)
                {
                    foreach (var tile in targets)
                    {
                        var canCastTile = customizableCondition.DoesTileMatchConditionFullParameters(caster?.Tile, tile);
                        if(!canCastTile) return false;
                    }
                    
                    return true;
                }
            }
            
            void AddEffect(DynValue dynValue)
            {
                var index = 0;
                switch (dynValue.Type)
                {
                    case DataType.Number:
                        index = (int) dynValue.Number;
                        if (index < 0 || index >= availableSelectionConditionalEffects.Count) return;
                        break;
                    case DataType.String:
                        Debug.Log($"Lua : {dynValue.String}");
                        return;
                    default:
                        return;
                }
                
                effects.AddRange(availableSelectionConditionalEffects[index].EffectsOnTarget);
            }
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


