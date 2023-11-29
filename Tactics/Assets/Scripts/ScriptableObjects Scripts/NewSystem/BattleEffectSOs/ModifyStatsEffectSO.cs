using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Effect
{
    public class ModifyStatsEffectSO : EffectSO
    {
        public override IEnumerable<string> SpecificParameters => Enum.GetNames(typeof(UnitStatsInstance.UnitStat));
        
        protected override void Effect(NewUnit caster, NewTile[] targetTiles, Func<string, dynamic> parameterGetter)
        {
            
        }
        
        private struct OperationData
        {
            public bool use;
            public UnitStatsInstance.UnitStat stat;
            public Operation operation;
            public float value;
            
            public string debug;
        }
        
        protected override object InterpretParameter(string parameter, string value, bool wasNotFound)
        {
            var result = new OperationData
            {
                use = false,
                value = 0f
            };
            
            if (wasNotFound) return result;
            
            var invalidStat = !Enum.TryParse(parameter, out result.stat);
            
            if (invalidStat) return result; //invalid Stat
            
            var operationText = value[0].ToString();
            var valueText = value[1..];
            
            var invalidOperation = TextToOperation(operationText, out result.operation);

            if (invalidOperation)
            {
                result.operation = Operation.Set;
                valueText = value;
            }
            
            var invalidValue = !float.TryParse(valueText, out result.value);
            
            if(invalidValue) return result;
            
            result.use = true;

            return result;
        }
    }
}


