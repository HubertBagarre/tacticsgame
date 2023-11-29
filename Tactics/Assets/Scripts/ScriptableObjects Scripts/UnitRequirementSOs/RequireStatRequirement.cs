using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Requirement
{
    [Serializable]
    public class RequiredStat
    {
        [field: SerializeField] public UnitStatsInstance.UnitStat Stat { get; private set; }
        [field: SerializeField] public int RequiredValue { get; private set; }
        [field: SerializeField] public bool RequireMaxValue { get; private set; }
        [field: SerializeField] public bool ConsumeStat { get; private set; } = false;
        [field: SerializeField] public bool ConsumeCurrentValue { get; private set; } = false;
    }
    
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Requirement/Required Stat")]
    public class RequireStatRequirement : UnitAbilityRequirementSO
    {
        [SerializeField] private List<RequiredStat> requiredStats = new ();
        private IEnumerable<RequiredStat> ConsumedStats => requiredStats.Where(stat => stat.ConsumeStat);
        private IEnumerable<RequiredStat> RequiredStats => requiredStats.Where(stat => !stat.ConsumeStat);

        public override List<(string verb, string content)> Descriptions(Tile tile)
        {
            var returnList = new List<(string verb, string content)>();
            
            var requiredText = GetStatsText(RequiredStats);
            if(requiredText != string.Empty) returnList.Add(("Requires",requiredText));

            var consumedText = GetStatsText(ConsumedStats);
            if(consumedText != string.Empty) returnList.Add(("Consumes",consumedText));

            return returnList;
            
            string GetStatsText(IEnumerable<RequiredStat> enumerable)
            {
                var list = enumerable.ToList();
                
                if (list.Count == 0) return string.Empty;
                
                var passivesCount = list.Count;
                
                var requiredStat = list[0];

                var enumerableText = GetStatText(requiredStat);

                if (passivesCount >= 2)
                {
                    for (var i = 1; i < passivesCount; i++)
                    {
                        requiredStat = list[i];

                        enumerableText += i == passivesCount - 1 ? " and" : ",";
                        enumerableText += GetStatText(requiredStat);
                        
                    }
                }
                
                return enumerableText;
            }
            
            string GetStatText(RequiredStat requiredStat)
            {
                var stat = requiredStat.Stat;
                var statText = stat switch
                {
                    UnitStatsInstance.UnitStat.Hp => "Hp",
                    UnitStatsInstance.UnitStat.MaxHp => "Max Hp",
                    UnitStatsInstance.UnitStat.Movement => "Max Movement",
                    UnitStatsInstance.UnitStat.CurrentMovement => "Movement",
                    UnitStatsInstance.UnitStat.Speed => "Speed",
                    UnitStatsInstance.UnitStat.Attack => "Attack",
                    UnitStatsInstance.UnitStat.AttackRange => "Attack Range",
                    UnitStatsInstance.UnitStat.MaxShield => "Max Shield",
                    UnitStatsInstance.UnitStat.CurrentShield => "Shield",
                    _ => string.Empty
                };
                
                var amountText = $"{requiredStat.RequiredValue} ";
                if(requiredStat.RequireMaxValue && HasNoMaxValue(stat)) amountText = string.Empty;
                if(requiredStat.RequireMaxValue && !HasNoMaxValue(stat)) amountText = "max ";
                if(requiredStat.RequireMaxValue && requiredStat.ConsumeStat) amountText = "all ";
                if (requiredStat.ConsumeCurrentValue && requiredStat.ConsumeStat) amountText = "current ";
                
                return $"{amountText}{statText}";
            }
        }
        
        private static int GetStatValue(Unit unit,UnitStatsInstance.UnitStat stat)
        {
            var stats = unit.Stats;
            return stat switch
            {
                UnitStatsInstance.UnitStat.Hp => stats.CurrentHp,
                UnitStatsInstance.UnitStat.MaxHp => stats.MaxHp,
                UnitStatsInstance.UnitStat.Movement => stats.Movement,
                UnitStatsInstance.UnitStat.CurrentMovement => unit.MovementLeft,
                UnitStatsInstance.UnitStat.Speed => stats.Speed,
                UnitStatsInstance.UnitStat.Attack => stats.Attack,
                UnitStatsInstance.UnitStat.AttackRange => stats.AttackRange,
                UnitStatsInstance.UnitStat.MaxShield => stats.MaxShield,
                UnitStatsInstance.UnitStat.CurrentShield => stats.CurrentShield,
                _ => 0
            };
        }

        private static int GetStatMaxValue(Unit unit,UnitStatsInstance.UnitStat stat)
        {
            var stats = unit.Stats;
            return stat switch
            {
                UnitStatsInstance.UnitStat.Hp => stats.MaxHp,
                UnitStatsInstance.UnitStat.CurrentMovement => stats.Movement,
                UnitStatsInstance.UnitStat.CurrentShield => stats.MaxShield,
                _ => 0
            };
        }
        
        private static bool HasStat(Unit unit, RequiredStat requiredStat)
        {
            var requiredValue = requiredStat.RequiredValue;
            var stat = requiredStat.Stat;

            if (requiredStat.ConsumeStat && requiredStat.ConsumeCurrentValue) requiredValue = 1;
            
            if (requiredStat.RequireMaxValue)
            {
                if (HasNoMaxValue(stat)) return true;
                requiredValue = GetStatMaxValue(unit,stat);
            }
            
            var currentValue = GetStatValue(unit,requiredStat.Stat);
            
            return currentValue >= requiredValue;
        }

        private static bool HasNoMaxValue(UnitStatsInstance.UnitStat stat) => stat switch
        {
            UnitStatsInstance.UnitStat.Hp => false,
            UnitStatsInstance.UnitStat.MaxHp => true,
            UnitStatsInstance.UnitStat.Movement => true,
            UnitStatsInstance.UnitStat.CurrentMovement => false,
            UnitStatsInstance.UnitStat.Speed => true,
            UnitStatsInstance.UnitStat.Attack => true,
            UnitStatsInstance.UnitStat.AttackRange => true,
            UnitStatsInstance.UnitStat.MaxShield => true,
            UnitStatsInstance.UnitStat.CurrentShield => false,
            _ => true
        };
        
        public override bool CanCastAbility(Tile tile)
        {
            if(!tile.HasUnit()) return false;

            var unit = tile.Unit;
            
            foreach (var requiredPassive in requiredStats)
            {
                if (!HasStat(unit,requiredPassive)) return false;
            }

            return true;
        }

        public override void ConsumeRequirement(Tile tile)
        {
            if(!tile.HasUnit()) return; 
            
            var unit = tile.Unit;
            
            foreach (var requiredStat in requiredStats.Where(requiredStat => requiredStat.ConsumeStat))
            {
                if (HasStat(unit,requiredStat)) ConsumeStats(requiredStat);
            }
            return; 
            
            void ConsumeStats(RequiredStat requiredStat)
            {
                var consumedValue = requiredStat.RequiredValue;
                var currentValue = requiredStat.ConsumeCurrentValue;
                
                var stats = unit.Stats;
                if (requiredStat.RequireMaxValue)
                {
                    var stat = requiredStat.Stat;
                    //if no max value, current value = true;
                    if (HasNoMaxValue(stat)) currentValue = true;
                    
                    //if max value, consumed value = max value
                    if (!currentValue)
                    {
                        consumedValue = GetStatMaxValue(unit,stat);
                    }
                }
                
                switch (requiredStat.Stat)
                {
                    case UnitStatsInstance.UnitStat.Hp:
                        unit.TakeHpDamage(currentValue ? stats.CurrentHp : consumedValue);
                        return;
                    case UnitStatsInstance.UnitStat.MaxHp:
                        stats.IncreaseMaxHpModifier(-(currentValue ? stats.MaxHp : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.Movement:
                        stats.IncreaseMovementModifier(-(currentValue ? stats.Movement : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.CurrentMovement:
                        unit.DecreaseMovement(currentValue ? unit.MovementLeft : consumedValue);
                        return;
                    case UnitStatsInstance.UnitStat.Speed:
                        stats.IncreaseSpeedModifier(-(currentValue ? stats.Speed : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.Attack:
                        stats.IncreaseAttackModifier(-(currentValue ? stats.Attack : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.AttackRange:
                        stats.IncreaseAttackRangeModifier(-(currentValue ? stats.AttackRange : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.MaxShield:
                        stats.IncreaseMaxShieldModifier(-(currentValue ? stats.MaxShield : consumedValue));
                        return;
                    case UnitStatsInstance.UnitStat.CurrentShield:
                        stats.CurrentShield -= (currentValue ? stats.CurrentShield : consumedValue);
                        return;
                    default:
                        return;
                }
            }
        }
    }
}


