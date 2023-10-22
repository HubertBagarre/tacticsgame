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
            [field: SerializeField] public PassiveSO<Unit> UnitPassive { get; private set; }
            public bool HasUnitPassive => UnitPassive != null;
            [field: SerializeField] public PassiveSO<Tile> TilePassive { get; private set; }
            public bool HasTilePassive => TilePassive != null;
            [field: SerializeField,Min(0)] public int RequiredStacks { get; private set; } = 0;
            public bool RequiresStacks()
            {
                if(RequiredStacks <= 0) return false;
                if(HasUnitPassive) return UnitPassive.IsStackable;
                if(HasTilePassive) return TilePassive.IsStackable;
                return false;
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
            
            var passive = requiredPassives[passiveIndex].UnitPassive;

            text = passive.Description;
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

                return $"<color=yellow>{amountText} <u><link=\"passive:{0}\">{requiredPassive.UnitPassive.Name}</link></u></color>";
            }
        }
        
        private static bool Condition<T>(PassiveInstance<T> instance,RequiredPassive requiredPassive) where T : IPassivesContainer<T>
        {
            var matchingSo = instance.SO == requiredPassive.UnitPassive;
            if(!matchingSo) return false;
                    
            var requireStacks = requiredPassive.RequiresStacks();

            if (!requireStacks) return true;
                    
            return instance.CurrentStacks >= requiredPassive.RequiredStacks;
        }
        
        public override bool CanCastAbility(Tile tile)
        {
            var unit = tile.Unit;
            foreach (var requiredPassive in requiredPassives)
            {
                if(requiredPassive.HasTilePassive)
                {
                    if(tile.GetPassiveEffectCount(TileCondition, out _) == 0) return false;
                }

                if(requiredPassive.HasUnitPassive && unit != null)
                {
                    if(unit.GetPassiveEffectCount(UnitCondition, out _) == 0) return false;
                }
                
                continue;

                bool TileCondition(PassiveInstance<Tile> instance) => Condition(instance, requiredPassive);
                bool UnitCondition(PassiveInstance<Unit> instance) => Condition(instance, requiredPassive);
            }

            return true;
        }

        public override IEnumerator ConsumeRequirement(Tile tile)
        {
            var unit = tile.Unit;
            foreach (var requiredPassive in ConsumedPassives)
            {
                if(requiredPassive.TilePassive != null)
                {
                    tile.GetPassiveEffectCount(TileCondition, out var passiveInstance);
                    if(passiveInstance != null) yield return tile.StartCoroutine(passiveInstance.DecreaseStacks(requiredPassive.RequiredStacks));
                }

                if (requiredPassive.UnitPassive != null && unit != null)
                {
                    unit.GetPassiveEffectCount(UnitCondition, out var passiveInstance);
                    if(passiveInstance != null) yield return unit.StartCoroutine(passiveInstance.DecreaseStacks(requiredPassive.RequiredStacks));
                }
                
                continue;

                bool TileCondition(PassiveInstance<Tile> instance) => Condition(instance, requiredPassive);
                bool UnitCondition(PassiveInstance<Unit> instance) => Condition(instance, requiredPassive);
            }
        }
    }
}