using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Conditions
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Condition/Unit Condition")]
    public class UnitConditionSO : AbilityConditionSO
    {
        [SerializeField] private bool targetAllies = false;
        [SerializeField] private bool targetEnemies = false;
        
        protected override IEnumerable<string> SpecificParameters => Array.Empty<string>();
        protected override bool CheckTile(NewTile referenceTile, NewTile tileToCheck, Func<string,dynamic> parameterGetter)
        {
            if (!tileToCheck.HasUnit) return false;
            
            return CheckUnit(referenceTile,tileToCheck.Unit,parameterGetter);
        }
        
        protected override string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter)
        {
            return "";
        }
        
        protected virtual bool CheckUnit(NewTile referenceTile, NewUnit unit,Func<string,dynamic> parameterGetter)
        {
            return true;
        }

        protected override (string targetText, string countText) TargetOverride(NewTile referenceTile,int count, Func<string,dynamic> parameterGetter)
        {
            var targetText = $"unit{(count > 1 ? "s" : "")}";
            
            if (targetAllies || !targetEnemies) targetText = $"all{(count > 1 ? "ies" : "y")}";
            if (targetEnemies || !targetAllies) targetText=  $"enem{(count > 1 ? "ies" : "y")}";

            var value = (targetText, $"{count}");
            
            return value;
        }

        protected override object InterpretParameter(string parameter, string value, bool defaultValue)
        {
            return null;
        }
    }
}

