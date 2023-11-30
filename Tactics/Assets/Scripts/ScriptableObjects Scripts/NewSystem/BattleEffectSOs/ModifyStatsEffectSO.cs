using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Effect/Modify Stats")]
    public class ModifyStatsEffectSO : AbilityEffectSO
    {
        public override IEnumerable<string> SpecificParameters => Enum.GetNames(typeof(UnitStat));
        
        protected override void Effect(NewUnit caster, NewTile[] targetTiles, Func<string, dynamic> parameterGetter)
        {
            foreach (var parameter in SpecificParameters)
            {
                var operationData = parameterGetter.Invoke(parameter) as OperationData ? ?? new OperationData();
                
                if(operationData.use) ChangeStat(operationData);
            }
            
            return;

            void ChangeStat(OperationData operationData)
            {
                var stat = operationData.stat;
                var operation = operationData.operation;
                var value = operationData.value;
                
                foreach (var targetTile in targetTiles)
                {
                    var unit = targetTile.Unit;

                    unit?.Stats.ModifyStat(stat,operation,value);
                }
            }
        }
        
        private struct OperationData
        {
            public bool use;
            public UnitStat stat;
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
            
            var validStat = Enum.TryParse(parameter, out result.stat);
            
            if (!validStat) return result; //invalid Stat
            
            var operationText = value[0].ToString();
            var valueText = value[1..];
            
            var validOperation = TextToOperation(operationText, out result.operation);
            
            result.debug = $"{result.operation}";

            if (!validOperation)
            {
                result.operation = Operation.Set;
                valueText = value;
            }
            
            var validValue = float.TryParse(valueText, out result.value);
            
            if(!validValue) return result;
            
            result.use = true;

            return result;
        }

        protected override string Text(NewTile referenceTile, Func<string, dynamic> parameterGetter)
        {
            return GenerateListOfParametersText<OperationData>(parameterGetter, GenerateOperationText);
            
            (bool,string) GenerateOperationText(OperationData operationData)
            {
                if (!operationData.use) return (false,string.Empty);
                
                var stat =  UnitStatsInstance.UnitStatToText(operationData.stat);
                var operation = operationData.operation;
                var value = Mathf.FloorToInt(operationData.value);

                string text;
                string verb;
                
                switch (operation)
                {
                    case Operation.Set:
                        text = $"set it's {stat} to {value} ({operationData.debug})";
                        return (true,text);
                    case Operation.Add:
                        verb = "increase";
                        break;
                    case Operation.Subtract:
                        verb = "decrease";
                        break;
                    case Operation.Multiply:
                        verb = "multiply";
                        break;
                    case Operation.Divide:
                        verb = "divide";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                text = $"{verb} it's {stat} by {value}";

                return (true, text);
            }
        }
    }
}


