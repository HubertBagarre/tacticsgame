using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Conditions
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Condition/Range Condition")]
    public class InRangeConditionSO : AbilityConditionSO
    {
        public override IEnumerable<string> SpecificParameters => new[] {"Range"};
        
        protected override bool DoesTileMatchCondition(NewTile referenceTile, NewTile tileToCheck, Func<string,dynamic> parameterGetter)
        {
            var distance = tileToCheck.Position - referenceTile.Position;
            distance = new Vector2Int(Mathf.Abs(distance.x),Mathf.Abs(distance.y));
            
            //tileToCheck.TileRenderer.DebugText.text = $"{distance.x},{distance.y}";

            var range = parameterGetter.Invoke("Range") as Vector2Int? ?? Vector2Int.zero;
            
            if(range.x > range.y) range = new Vector2Int(range.y,range.x);
            var minDistance = range.x;
            var maxDistance = range.y;
            
            return distance.x >= minDistance && distance.x <= maxDistance && distance.y >= minDistance && distance.y <= maxDistance;
        }
        
        protected override string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter)
        {
            var range = parameterGetter.Invoke("Range") as Vector2Int? ?? Vector2Int.zero;
            
            if(range.x > range.y) range = new Vector2Int(range.y,range.x);
            var minDistance = range.x;
            var maxDistance = range.y;
            
            if (minDistance == 0)
            {
                return $"within {RingText(maxDistance)}";
            }
            
            return  $"between {RingText(minDistance)} and {RingText(maxDistance)}";

            string RingText(int count)
            {
                return $"<u><link=\"ring:{count}\">{count} ring{(count > 1 ? "s" : "")}</link></u>";
            }
        }
        protected override object InterpretParameter(string parameter, string value, bool useDefaultValue)
        {
            if (parameter != "Range") return null;
            
            var defaultValue = Vector2Int.zero;

            if (string.IsNullOrEmpty(value)) return defaultValue;
            
            var split = value.Split(',');
                
            if (!int.TryParse(split[0], out var min)) return defaultValue;
            
            if(split.Length <= 1) return new Vector2Int(0,min);
            
            if (!int.TryParse(split[1], out var max)) return defaultValue;
                
            return new Vector2Int(min,max);

        }
    }
}
