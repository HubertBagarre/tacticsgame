using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Requirement
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Requirement/Required Passives")]
    public class RequirePassiveRequirement : UnitAbilityRequirementSO
    {
        [Serializable]
        public class RequiredPassive
        {
            [field: SerializeField] public PassiveSO PassiveSo { get; private set; }
            [field: SerializeField,Min(0)] public int RequiredStacks { get; private set; } = 0;
            public bool RequiresStacks()
            {
                if(RequiredStacks <= 0) return false;
                return PassiveSo.IsStackable;
            }

            public string PassiveDescription()
            {
                return PassiveSo.Description;
            }

            [field: SerializeField] public bool ConsumeStacks { get; private set; } = false;
        }
        
        [SerializeField] private List<RequiredPassive> requiredPassives = new ();
        private IEnumerable<RequiredPassive> ConsumedPassives => requiredPassives.Where(passive => passive.ConsumeStacks);
        private IEnumerable<RequiredPassive> RequiredPassives => requiredPassives.Where(passive => !passive.ConsumeStacks);
        
        public override bool ConvertDescriptionLinks(Tile tile, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("passive:")) return false;
            
            var split = linkKey.Split(":");

            if (!int.TryParse(split[1], out var passiveIndex)) return false;
            
            text =requiredPassives[passiveIndex].PassiveDescription();
            
            return true;
        }

        public override List<(string verb, string content)> Descriptions(Tile tile)
        {
            var returnList = new List<(string verb, string content)>();
            
            var requiredText = GetPassivesText(RequiredPassives);
            if(requiredText != string.Empty) returnList.Add(("Requires",requiredText));

            var consumedText = GetPassivesText(ConsumedPassives);
            if(consumedText != string.Empty) returnList.Add(("Consumes",consumedText));

            return returnList;
            
            string GetPassivesText(IEnumerable<RequiredPassive> enumerable)
            {
                var list = enumerable.ToList();
                
                if (list.Count == 0) return string.Empty;
                
                var passivesCount = list.Count;
                
                var requiredPassive = list[0];

                var enumerableText = GetPassiveText(requiredPassive);

                if (passivesCount >= 2)
                {
                    for (var i = 1; i < passivesCount; i++)
                    {
                        requiredPassive = list[i];

                        enumerableText += i == passivesCount - 1 ? " and" : ",";
                        enumerableText += GetPassiveText(requiredPassive);
                    }
                }
                return enumerableText;
            }

            string GetPassiveText(RequiredPassive requiredPassive)
            {
                var amountText = requiredPassive.RequiresStacks()
                    ? $"{requiredPassive.RequiredStacks} stack{(requiredPassive.RequiredStacks > 1 ? "s" : "")} of "
                    : "";

                var passiveName = string.Empty;
                passiveName = requiredPassive.PassiveSo.Name;
                
                return $"<color=yellow>{amountText} <u><link=\"passive:{0}\">{passiveName}</link></u></color>";
            }
        }
        
        private static bool Condition(PassiveInstance instance,RequiredPassive requiredPassive)
        {
            var matchingSo = false;
            matchingSo = instance.SO == requiredPassive.PassiveSo;
            
            if(!matchingSo) return false;
            
            var requireStacks = requiredPassive.RequiresStacks();

            //Debug.Log($"Found Matching SO ({instance.SO.Name}), requiresStacks : {requireStacks}");
            
            if (!requireStacks) return true;
                    
            //Debug.Log($"CurrentStacks : {instance.CurrentStacks}, Required Stacks : {requiredPassive.RequiredStacks}");
            
            return instance.CurrentStacks >= requiredPassive.RequiredStacks;
        }
        
        public override bool CanCastAbility(Tile tile)
        {
            var unit = tile.Unit;
            foreach (var requiredPassive in requiredPassives)
            {
                if (unit.GetPassiveEffectCount(UnitCondition, out _) == 0)
                {
                    //Debug.Log($"Missing Unit Passive : {requiredPassive.UnitPassive.Name}");
                    return false;
                }
                continue;

                bool UnitCondition(PassiveInstance instance) => Condition(instance, requiredPassive);
            }

            return true;
        }

        public override void ConsumeRequirement(Tile tile)
        {
            var unit = tile.Unit;
            foreach (var requiredPassive in ConsumedPassives)
            {
                if (requiredPassive.PassiveSo != null && unit != null)
                {
                    unit.GetPassiveEffectCount(UnitCondition, out var passiveInstance);
                    passiveInstance?.RemoveStacks(requiredPassive.RequiredStacks);
                }
                
                continue;
                
                bool UnitCondition(PassiveInstance instance) => Condition(instance, requiredPassive);
            }
        }
    }
}