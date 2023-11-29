using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
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
        
        public string EffectsText(NewTile referenceTile)
        {
            if (typeof(T).IsSubclassOf(typeof(AbilityEffectSO))) return "<i>nothing happens</i>";
            
            var count = Effects.Count;
            
            if(count <= 0) return string.Empty;
            
            var effectList = Effects.Cast<AbilityEffectSO>().ToList();

            var text = $"{effectList[0].TextFullParameters(referenceTile, Parameters)}.";
            text = char.ToUpper(text[0]) + text[1..];

            if (count == 1) return text;

            for (int i = 1; i < effectList.Count; i++)
            {
                var effect = effectList[i];
                var effectText = $"{effect.TextFullParameters(referenceTile, Parameters)}.";
                effectText = char.ToUpper(effectText[0]) + effectText[1..];
                
                text += "\n";
                text += effectText;
            }
            
            return text;
        }
    }
    
    public abstract class EffectSO : ParametableSO
    {
        public void EffectFullParameters(NewUnit caster, NewTile[] targetTiles, string parameters)
        {
            Effect(caster, targetTiles, LocalGetParameterValue);

            return;

            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }

        protected abstract void Effect(NewUnit caster, NewTile[] targetTiles, Func<string, dynamic> parameterGetter);
    }

    public abstract class AbilityEffectSO : EffectSO
    {
        public string TextFullParameters(NewTile referenceTile,string parameters)
        {
            return Text(referenceTile,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        /// <summary>
        /// text should be used in this context : "Select target,{Text()},{Text()} and {Text()}"
        /// </summary>
        protected abstract string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter);
    }
}
