using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [Serializable]
    public class CustomizableCondition<T> where T : ConditionSO
    {
        [field: TextArea(1,10),Tooltip(ParametableSO.ToolTipText)]
        [field:SerializeField] public string Parameters { get; private set; }

        [SerializeField] private T[] conditions;
        public IReadOnlyCollection<T> Conditions => conditions;
        
        public bool DoesTileMatchConditionFullParameters(NewTile referenceTile,NewTile tileToCheck)
        {
            if(Conditions.Count <= 0) return true;
            
            foreach (var condition in Conditions)
            {
                if(!condition.DoesTileMatchConditionFullParameters(referenceTile,tileToCheck,Parameters)) return false;
            }

            return true;
        }
    }
    
    [Serializable]
    public class CustomizableAbilityCondition : CustomizableCondition<AbilityConditionSO>
    {
        public string ConditionText(NewTile referenceTile,int count)
        {
            if(Conditions.Count <= 0) return string.Empty;
            
            var text = "%COUNT% %TARGET%";
            
            foreach (var requirement in Conditions)
            {
                text += requirement.TextFullParameters(referenceTile,Parameters);
            }
            
            return OverrideTargetText(text,count);
        }

        /// <summary>
        /// Replaces the %TARGET% and %COUNT% in inputText with the correct text
        /// </summary>
        public string OverrideTargetText(string inputText,int count)
        {
            if(Conditions.Count <= 0) return inputText;
            
            (string targetText, string countText) texts = (string.Empty,$"{count}");
            
            foreach (var requirement in Conditions)
            {
                var req = requirement.TargetOverrideFullParameters(count, Parameters);
                if (req.targetText != string.Empty && texts.targetText == string.Empty) texts = req;
            }

            if (texts.targetText == string.Empty) texts.targetText = count > 1 ? "tiles" : "tile";
            
            inputText = inputText.Replace("%TARGET%",texts.targetText);
            inputText = inputText.Replace("%COUNT%",texts.countText);
            
            return inputText;
        }
    }
    
    public abstract class ConditionSO : ParametableSO
    {
        public bool DoesTileMatchConditionFullParameters(NewTile referenceTile,NewTile tileToCheck,string parameters)
        {
            return DoesTileMatchCondition(referenceTile,tileToCheck,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        protected abstract bool DoesTileMatchCondition(NewTile referenceTile,NewTile tileToCheck,Func<string,dynamic> parameterGetter);
    }
    
    public abstract class AbilityConditionSO : ConditionSO
    {
        protected virtual IEnumerable<string> LinkKeys => Array.Empty<string>();
        
        public string TextFullParameters(NewTile referenceTile,string parameters)
        {
            return Text(referenceTile,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        public (string targetText, string countText) TargetOverrideFullParameters(int count,string parameters)
        {
            return TargetOverride(count,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        // TODO same thing with the specific parameter (maybe make another class cuz this will be use by lots of stuff)
        protected bool ConvertDescriptionLinksFullParameters(NewTile referenceTile, string linkKey,string parameters, out string text)
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
        ///  text that will replace %TARGET% and %COUNT% in text
        /// </summary>
        protected virtual (string targetText, string countText) TargetOverride(int count,Func<string,dynamic> parameterGetter)
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

