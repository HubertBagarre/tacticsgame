using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Requirement/Required Passives")]
    public class RequirePassiveRequirement : UnitAbilityRequirementSO
    {
        [Serializable]
        public class RequiredPassive
        {
            [field: SerializeField] public PassiveSO<Unit> Passive { get; private set; }
            [field: SerializeField] public int RequiredStacks { get; private set; } = 0;
            public bool RequiresStacks => RequiredStacks > 0 && Passive.IsStackable;
            [field: SerializeField] public bool ConsumeStacks { get; private set; } = false;
        }
        
        [SerializeField] private List<RequiredPassive> requiredPassives = new ();
        private IEnumerable<RequiredPassive> ConsumedPassives => requiredPassives.Where(passive => passive.ConsumeStacks);
        private IEnumerable<RequiredPassive> RequiredPassives => requiredPassives.Where(passive => !passive.ConsumeStacks);
        
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("passive:")) return false;
            
            var split = linkKey.Split(":");

            if (!int.TryParse(split[1], out var passiveIndex)) return false;
            
            var passive = requiredPassives[passiveIndex].Passive;

            text = passive.Description;
            return true;
        }
        
        public override string Description(Unit caster)
        {
            var text = string.Empty;

            var requiredText = GetText(RequiredPassives, "Requires");
            var consumesText = GetText(ConsumedPassives, "Consumes");
            
            if(requiredText != string.Empty) text += requiredText;
            if(consumesText != string.Empty) text += consumesText;
            text = text.TrimEnd('\n');
            
            return text;
            
            string GetText(IEnumerable<RequiredPassive> enumerable,string enumerableText)
            {
                var list = enumerable.ToList();
                
                if (list.Count == 0) return string.Empty;
                
                var passivesCount = list.Count;
                
                var requiredPassive = list[0];
                    
                enumerableText += $"<color=yellow>{(requiredPassive.RequiresStacks ? $" {requiredPassive.RequiredStacks} stack{(requiredPassive.RequiredStacks > 1 ? "s":"")} of ":"")}" +
                                  $" <u><link=\"passive:{0}\">{requiredPassive.Passive.Name}</link></u></color>";

                if (passivesCount >= 2)
                {
                    for (var i = 1; i < passivesCount; i++)
                    {
                        requiredPassive = list[i];

                        enumerableText += i == passivesCount - 1 ? " and" : ",";
                        enumerableText += $"<color=yellow>{(requiredPassive.Passive.IsStackable ? $" {requiredPassive.RequiredStacks} stack{(requiredPassive.RequiredStacks > 1 ? "s":"")} of ":"")}" +
                                          $" <u><link=\"passive:{i}\">{requiredPassive.Passive.Name}</link></u></color>";
                    }
                }

                enumerableText += ".\n";
                return enumerableText;
            }
        }
        
        private static bool Condition(PassiveInstance<Unit>  instance,RequiredPassive requiredPassive)
        {
            var matchingSo = instance.SO == requiredPassive.Passive;
            if(!matchingSo) return false;
                    
            var requireStacks = requiredPassive.RequiresStacks;

            if (!requireStacks) return true;
                    
            return instance.CurrentStacks >= requiredPassive.RequiredStacks;
        }
        
        public override bool CanCastAbility(Unit caster)
        {
            foreach (var requiredPassive in requiredPassives)
            {
                if (caster.GetPassiveEffectCount(Func, out _) == 0) return false;
                
                continue;

                bool Func(PassiveInstance<Unit> instance) => Condition(instance, requiredPassive);
            }

            return true;
        }

        public override IEnumerator ConsumeRequirement(Unit caster)
        {
            foreach (var requiredPassive in ConsumedPassives)
            {
                caster.GetPassiveEffectCount(Func, out var passiveInstance);
                
                if(passiveInstance != null) yield return caster.StartCoroutine(passiveInstance.DecreaseStacks(requiredPassive.RequiredStacks));
                
                continue;

                bool Func(PassiveInstance<Unit> instance) => Condition(instance, requiredPassive);
            }
        }
    }
}