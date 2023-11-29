using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Conditions
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Condition/Stats Condition")]
    public class StatsConditionSO : UnitConditionSO
    {
        public override IEnumerable<string> SpecificParameters => Enum.GetNames(typeof(UnitStatsInstance.UnitStat));

        protected override bool CheckUnit(NewTile referenceTile, NewUnit unit, Func<string, dynamic> parameterGetter)
        {
            if (!base.CheckUnit(referenceTile, unit, parameterGetter)) return false;

            foreach (var parameter in SpecificParameters)
            {
                var comparison = parameterGetter.Invoke(parameter) as ComparisonData ? ?? new ComparisonData();
                
                if (comparison.use && !CheckComparison(comparison)) return false;
            }
            
            return true;
            
            bool CheckComparison(ComparisonData comparisonData)
            {
                var statValue = unit.Stats.GetStat(comparisonData.stat);   
                var refValue = comparisonData.valueToCompareTo;
                
                if (comparisonData.useStatToCompareTo)
                {
                    var unitToCompareTo = comparisonData.compareWithCaster ? referenceTile.Unit : unit;
                    
                    if(unitToCompareTo == null) return false;
                    
                    refValue = unitToCompareTo.Stats.GetStat(comparisonData.statToCompareTo);
                }

                return comparisonData.comparison switch
                {
                    Comparison.Equal => statValue == refValue,
                    Comparison.NotEqual => statValue != refValue,
                    Comparison.Greater => statValue > refValue,
                    Comparison.GreaterOrEqual => statValue >= refValue,
                    Comparison.Lesser => statValue < refValue,
                    Comparison.LesserOrEqual => statValue <= refValue,
                    _ => false
                };
            }
        }
        
        protected override string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter)
        {
            var returnText = " with ";
            
            var list = new List<string>();
            
            foreach (var parameter in SpecificParameters)
            {
                var comparison = parameterGetter.Invoke(parameter) as ComparisonData ? ?? new ComparisonData();

                if (comparison.use) list.Add($"{GenerateComparisonText(comparison)}");
            }
            
            if(list.Count == 0) return string.Empty;

            returnText += list[0];
            
            if (list.Count == 1) return returnText;
            
            for (int i = 1; i < list.Count; i++)
            {
                var text = list[i];
                var separator = i != list.Count-1 ? ", " : " and ";
                returnText += $"{separator}{text}";
            }
            
            return returnText;
            
            string GenerateComparisonText(ComparisonData comparisonData)
            {
                var preValue = "";
                var postValue = "";
                var value = $"{comparisonData.valueToCompareTo}";

                if (comparisonData.useStatToCompareTo)
                {
                    value = UnitStatsInstance.UnitStatToText(comparisonData.statToCompareTo);
                    switch (comparisonData.comparison)
                    {
                        case Comparison.Equal:
                            preValue = "as much ";
                            postValue = " as";
                            break;
                        case Comparison.NotEqual:
                            preValue = "different ";
                            postValue = " and";
                            break;
                        case Comparison.Greater:
                            preValue = "more ";
                            postValue = " than";
                            break;
                        case Comparison.GreaterOrEqual:
                            if (comparisonData.compareWithCaster) preValue = "greater or equal ";
                            postValue = " greater or equal to";
                            break;
                        case Comparison.Lesser:
                            preValue = "less ";
                            postValue = " than";
                            break;
                        case Comparison.LesserOrEqual:
                            if (comparisonData.compareWithCaster) preValue = "lesser or equal ";
                            postValue = " lesser or equal to";
                            break;
                    }
                }
                else
                {
                    switch (comparisonData.comparison)
                    {
                        case Comparison.Equal:
                            break;
                        case Comparison.NotEqual:
                            preValue = "not ";
                            break;
                        case Comparison.Greater:
                            preValue = "more than ";
                            break;
                        case Comparison.GreaterOrEqual:
                            postValue = " or more";
                            break;
                        case Comparison.Lesser:
                            preValue = "less than ";
                            break;
                        case Comparison.LesserOrEqual:
                            postValue = " or less";
                            break;
                    }
                }
                
                var statText = UnitStatsInstance.UnitStatToText(comparisonData.stat);

                var postStatText = comparisonData.compareWithCaster ? "" : $"{postValue} {statText}";
                
                return $"{preValue}{value}{postStatText}";
            }
        }
        
        private struct ComparisonData
        {
            public bool use;
            public UnitStatsInstance.UnitStat stat;
            public Comparison comparison;
            public int valueToCompareTo;
            
            public bool useStatToCompareTo;
            public bool compareWithCaster;
            public UnitStatsInstance.UnitStat statToCompareTo;

            public string debug;
        }

        protected override object InterpretParameter(string parameter, string value, bool parameterNotFound)
        {
            var result = new ComparisonData
            {
                use = false,
                useStatToCompareTo = false,
                compareWithCaster = false,
            };

            if (parameterNotFound) return result;
            
            if (!Enum.TryParse(parameter, out UnitStatsInstance.UnitStat resultStat)) return result; //invalid Stat
            
            result.stat = resultStat;
            
            var split = value.Split(' ');
            var operationText = ComparisonToText(Comparison.GreaterOrEqual);
            var valueToCompareToText = split[0];
            
            if(split.Length > 1)
            {
                operationText = split[0];
                valueToCompareToText = split[1];
            }
            
            if (!TextToComparison(operationText, out result.comparison)) return result; //invalid Comparison
            
            result.use = true;
            
            if (int.TryParse(valueToCompareToText, out var compareValue))
            {
                result.valueToCompareTo = compareValue;
                
                return result; // value is an int (use)
            }
            
            result.useStatToCompareTo = true;
            
            result.compareWithCaster = !Enum.TryParse(valueToCompareToText, out UnitStatsInstance.UnitStat compareStat);
            
            result.statToCompareTo = compareStat;
            
            if (split[0].Length != 0 && result.compareWithCaster) operationText = split[0];
            
            //result.debug = $"{operationText}";
            
            if (!TextToComparison(operationText, out result.comparison))
            {
                //result.debug = $"{operationText} (invalid)";
                result.use = false;
                return result; //invalid Comparison
            }
            
            if(result.statToCompareTo == resultStat && !result.compareWithCaster)
            {
                result.use = false;
                return result; // redundant comparison
            }
                
            (result.statToCompareTo, result.stat) = (result.stat, result.statToCompareTo); // flip stats (to match text)
                
            return result; // value is a stat (use)
        }
    }
}


