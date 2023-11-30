using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Components
{
    [Serializable]
    public struct ConditionalEffects<T> where T : AbilityEffectSO
    {
        [SerializeField] private BranchedConditionalEffect<T>[] conditionalEffects;
        public IReadOnlyCollection<BranchedConditionalEffect<T>> ConditionalEffectsCollection => conditionalEffects;
    }
    
    [Serializable]
    public class BranchedConditionalEffect<T> where T : EffectSO
    {
        [field: SerializeField] public int Depth { get; private set; }
        [field: SerializeField] public BranchOperator BranchOperator { get; private set; }
        [SerializeField] private ConditionalEffect<T> conditionalEffect;
        public ConditionalEffect<T> ConditionalEffect => conditionalEffect;
    }
    
    [Serializable]
    public class ConditionalEffect<T> where T : EffectSO
    {
        [field: SerializeField] public CustomizableCondition<AbilityConditionSO> Condition { get; private set; }
        [SerializeField] private EffectsOnTarget<T>[] effects;
        public IReadOnlyCollection<EffectsOnTarget<T>> EffectsOnTarget => effects;

        public bool CanCastEffect(NewUnit caster, NewTile[] targetTiles)
        {
            if (!Condition.DoesTileMatchConditionFullParameters(caster.Tile, caster.Tile)) return false;
            
            return Condition.DoesTileMatchConditionFullParameters(caster.Tile,caster.Tile);
        }

        public string Text(NewTile referenceTile)
        {
            var effectsOnTargetTexts = new List<string>();

            if(EffectsOnTarget.Count <= 0) return string.Empty;
            
            foreach (var effectsOnTarget in EffectsOnTarget)
            {
                effectsOnTargetTexts.AddRange(effectsOnTarget.GetEffectsOnTargetTexts(referenceTile));
            }
            
            if(effectsOnTargetTexts.Count <= 0) return string.Empty;

            var text = effectsOnTargetTexts[0];

            if (effectsOnTargetTexts.Count == 1) return text;

            for (int i = 1; i < effectsOnTargetTexts.Count; i++)
            {
                var effectText = effectsOnTargetTexts[i];
                
                text += "\n";
                text += effectText;
            }
            
            return text;
        }
    }
    
    [Serializable]
    public class EffectsOnTarget<T> where T : EffectSO
    {
        [field: TextArea(1,10),Tooltip(ParametableSO.ToolTipText)]
        [field:SerializeField] public string Parameters { get; private set; }
        [SerializeField] private T[] effects;
        //[field: SerializeField] public AffectorSO Affector { get; private set; }
        public IReadOnlyList<T> Effects => effects;
        
        public void ApplyEffects(NewUnit caster, NewTile[] targetTiles)
        {
            if(Effects.Count <= 0) return;
            
            foreach (var effect in Effects)
            {
                effect.EffectFullParameters(caster,targetTiles,Parameters);
            }
        }
        
        public IReadOnlyList<string> GetEffectsOnTargetTexts(NewTile referenceTile)
        {
            const string defaultText = "<i>do something</i>";

            if (typeof(T).IsSubclassOf(typeof(AbilityEffectSO))) return new List<string>(){defaultText};
            
            var count = Effects.Count;
            
            if(count <= 0) return new List<string>(){defaultText};

            var returnList = new List<string>();
            var effectList = Effects.Cast<AbilityEffectSO>().ToList();
            
            foreach (var effect in effectList)
            {
                var effectText = $"{effect.TextFullParameters(referenceTile, Parameters)}.";
                effectText = char.ToUpper(effectText[0]) + effectText[1..];
                
                returnList.Add(effectText);
            }

            return returnList;
        }
    }
}