using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class ConditionSO : ScriptableObject
    {
        public abstract IEnumerable<string> SpecificParameters { get; }
        
        public bool CheckTileFullParameters(NewTile referenceTile,NewTile tileToCheck,IReadOnlyDictionary<string,string> parameters)
        {
            return CheckTile(referenceTile,tileToCheck,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        protected abstract bool CheckTile(NewTile referenceTile,NewTile tileToCheck,Func<string,dynamic> parameterGetter);
        
        protected IReadOnlyDictionary<string,string> GetSpecificParameter(IReadOnlyDictionary<string, string> allParameters)
        {
            var dict = new Dictionary<string,string>();
            foreach (var specificParameter in SpecificParameters)
            {
                if(allParameters.TryGetValue(specificParameter, out var parameter)) dict.Add(specificParameter,parameter);
            }

            return dict;
        }

        protected T GetParameterValue<T>(string parameter,IReadOnlyDictionary<string,string> specificParameters)
        {
            var parameterNotFound = !specificParameters.TryGetValue(parameter, out var value);
            
            return (T)InterpretParameter(parameter,value,parameterNotFound);
        }

        protected abstract object InterpretParameter(string parameter,string value,bool wasNotFound);
        
        protected enum Comparison
        {
            Equal,
            NotEqual,
            Greater,
            GreaterOrEqual,
            Lesser,
            LesserOrEqual
        }
        
        protected static string ComparisonToText(Comparison comparison)
        {
            return comparison switch
            {
                Comparison.Equal => "==",
                Comparison.NotEqual => "!=",
                Comparison.Greater => ">",
                Comparison.GreaterOrEqual => ">=",
                Comparison.Lesser => "<",
                Comparison.LesserOrEqual => "<=",
                _ => "=="
            };
        }
        
        protected static bool TextToComparison(string text,out Comparison comparison)
        {
            comparison = Comparison.Equal;
            switch (text)
            {
                case "==":
                    return true;
                case "!=":
                    comparison = Comparison.NotEqual;
                    return true;
                case ">":
                    comparison = Comparison.Greater;
                    return true;
                case ">=":
                    comparison = Comparison.GreaterOrEqual;
                    return true;
                case "<":
                    comparison = Comparison.Lesser;
                    return true;
                case "<=":
                    comparison = Comparison.LesserOrEqual;
                    return true;
                default:
                    return false;
            }
        }
    }
    
    public abstract class AbilityConditionSO : ConditionSO
    {
        protected virtual IEnumerable<string> LinkKeys => Array.Empty<string>();
        
        public string TextFullParameters(NewTile referenceTile,IReadOnlyDictionary<string,string> parameters)
        {
            return Text(referenceTile,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        public (string targetText, string countText) TargetOverrideFullParameters(NewTile referenceTile,int count,IReadOnlyDictionary<string,string> parameters)
        {
            return TargetOverride(referenceTile,count,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        // TODO same thing with the specific parameter (maybe make another class cuz this will be use by lots of stuff)
        protected bool ConvertDescriptionLinksFullParameters(NewTile referenceTile, string linkKey,IReadOnlyDictionary<string,string> parameters, out string text)
        {
            return ConvertDescriptionLinks(referenceTile,linkKey,LocalGetParameterValue,out text);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        /// <summary>
        /// text should be used in this context : "Select %TARGET%{Text()}" or "Requires %TARGET%{Text()}"
        /// </summary>
        protected abstract string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter);
        
        /// <summary>
        ///  text that will replace %TARGET% in text
        /// </summary>
        protected virtual (string targetText, string countText) TargetOverride(NewTile referenceTile,int count,Func<string,dynamic> parameterGetter)
        {
            return (string.Empty,string.Empty);
        }
        
        /// <summary>
        /// text that will display in toolbar when hovering text with linkKey
        /// </summary>
        protected virtual bool ConvertDescriptionLinks(NewTile referenceTile, string linkKey,Func<string,dynamic> parameterGetter, out string text)
        {
            text = string.Empty;
            return false;
        }
    }
}


