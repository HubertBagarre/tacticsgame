using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [Serializable]
    public struct BranchedConditionalEffect<T> where T : EffectSO
    {
        [field: SerializeField] public int Depth { get; private set; }
        [field: SerializeField] public BranchOperator BranchOperator { get; private set; }
        [field: SerializeField] public ConditionalEffect<T> Effect { get; private set; }
    }
    
    [Serializable]
    public struct ConditionalEffect<T> where T : EffectSO
    {
        [field: SerializeField] public CustomizableCondition<AbilityConditionSO> Condition { get; private set; }
        [field: SerializeField] public EffectsOnTarget<T>[] Effects { get; private set; }

        public bool CanCastEffect(NewUnit caster, NewTile[] targetTiles)
        {
            if (!Condition.DoesTileMatchConditionFullParameters(caster.Tile, caster.Tile)) return false;
            
            return Condition.DoesTileMatchConditionFullParameters(caster.Tile,caster.Tile);
        }
    }
    
    [Serializable]
    public struct EffectsOnTarget<T> where T : EffectSO
    {
        [field: TextArea(1,10),Tooltip(ParametableSO.ToolTipText)]
        [field:SerializeField] public string Parameters { get; private set; }
        [SerializeField] private T[] effects;
        //[field: SerializeField] public AffectorSO Affector { get; private set; }
        public IReadOnlyCollection<T> Effects => effects;
        
        public void ApplyEffects(NewUnit caster, NewTile[] targetTiles)
        {
            if(Effects.Count <= 0) return;
            
            foreach (var effect in Effects)
            {
                effect.EffectFullParameters(caster,targetTiles,Parameters);
            }
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
        
    }
}
