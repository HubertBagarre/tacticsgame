using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Components
{
    [Serializable]
    public class ConditionalEffect<T> : IIdHandler where T : EffectSO
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public CustomizableCondition<AbilityConditionSO> Condition { get; private set; }
        [SerializeField] private EffectsOnTarget<T>[] effects;
        public IReadOnlyCollection<EffectsOnTarget<T>> EffectsOnTarget => effects;

        public bool DoesTileMatchConditionFullParameters(NewUnit caster, IEnumerable<NewTile> targetTiles)
        {
            if (Condition == null) return true;
            
            foreach (var target in targetTiles)
            {
                if (!Condition.DoesTileMatchConditionFullParameters(caster.Tile, target)) return false;
            }

            return true;
        }
        
        public void ApplyEffects(NewUnit caster, IEnumerable<NewTile> targetTiles)
        {
            if(EffectsOnTarget.Count <= 0) return;
            
            var targetTilesArray = targetTiles.ToArray();
            
            if (!DoesTileMatchConditionFullParameters(caster, targetTilesArray)) return;
            
            foreach (var effectsOnTarget in EffectsOnTarget)
            {
                effectsOnTarget.ApplyEffects(caster, targetTilesArray);
            }
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
    public class EffectsOnTarget<T> : IIdHandler where T : EffectSO
    {
        [field: SerializeField] public string Id { get; private set; }
        
        [field: TextArea(1,10),Tooltip(ParametableSO.ToolTipText)]
        [field:SerializeField] public string Parameters { get; private set; }
        [SerializeField] private T[] effects;
        //[field: SerializeField] public AffectorSO Affector { get; private set; }
        public IReadOnlyList<T> Effects => effects;
        
        public EffectsOnTarget(string parameters, T[] effects)
        {
            Parameters = parameters;
            this.effects = effects;
        }
        
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