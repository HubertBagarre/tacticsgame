using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class ConditionSO : ScriptableObject
    {
        protected abstract IEnumerable<string> SpecificParameters { get; }
        
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
            var useDefault = !specificParameters.TryGetValue(parameter, out var value);
            
            return (T)InterpretParameter(parameter,value,useDefault);
        }

        protected abstract object InterpretParameter(string parameter,string value,bool useDefaultValue);
    }
    
    public abstract class AbilityConditionSO : ConditionSO
    {
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


