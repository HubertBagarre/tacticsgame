using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Requirement
{
    [Serializable]
    public class RequiredStat
    {
        [field: SerializeField] public UnitStat Stat { get; private set; }
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
                    UnitStat.Hp => "Hp",
                    UnitStat.MaxHp => "Max Hp",
                    UnitStat.Movement => "Max Movement",
                    UnitStat.CurrentMovement => "Movement",
                    UnitStat.Speed => "Speed",
                    UnitStat.Attack => "Attack",
                    UnitStat.AttackRange => "Attack Range",
                    UnitStat.MaxShield => "Max Shield",
                    UnitStat.CurrentShield => "Shield",
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
        
        private static int GetStatValue(Unit unit,UnitStat stat)
        {
            var stats = unit.Stats;
            return stat switch
            {
                UnitStat.Hp => stats.CurrentHp,
                UnitStat.MaxHp => stats.MaxHp,
                UnitStat.Movement => stats.Movement,
                UnitStat.CurrentMovement => unit.MovementLeft,
                UnitStat.Speed => stats.Speed,
                UnitStat.Attack => stats.Attack,
                UnitStat.AttackRange => stats.AttackRange,
                UnitStat.MaxShield => stats.MaxShield,
                UnitStat.CurrentShield => stats.CurrentShield,
                _ => 0
            };
        }

        private static int GetStatMaxValue(Unit unit,UnitStat stat)
        {
            var stats = unit.Stats;
            return stat switch
            {
                UnitStat.Hp => stats.MaxHp,
                UnitStat.CurrentMovement => stats.Movement,
                UnitStat.CurrentShield => stats.MaxShield,
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

        private static bool HasNoMaxValue(UnitStat stat) => stat switch
        {
            UnitStat.Hp => false,
            UnitStat.MaxHp => true,
            UnitStat.Movement => true,
            UnitStat.CurrentMovement => false,
            UnitStat.Speed => true,
            UnitStat.Attack => true,
            UnitStat.AttackRange => true,
            UnitStat.MaxShield => true,
            UnitStat.CurrentShield => false,
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
                    case UnitStat.Hp:
                        unit.TakeHpDamage(currentValue ? stats.CurrentHp : consumedValue);
                        return;
                    case UnitStat.MaxHp:
                        stats.IncreaseMaxHpModifier(-(currentValue ? stats.MaxHp : consumedValue));
                        return;
                    case UnitStat.Movement:
                        stats.IncreaseMovementModifier(-(currentValue ? stats.Movement : consumedValue));
                        return;
                    case UnitStat.CurrentMovement:
                        unit.DecreaseMovement(currentValue ? unit.MovementLeft : consumedValue);
                        return;
                    case UnitStat.Speed:
                        stats.IncreaseSpeedModifier(-(currentValue ? stats.Speed : consumedValue));
                        return;
                    case UnitStat.Attack:
                        stats.IncreaseAttackModifier(-(currentValue ? stats.Attack : consumedValue));
                        return;
                    case UnitStat.AttackRange:
                        stats.IncreaseAttackRangeModifier(-(currentValue ? stats.AttackRange : consumedValue));
                        return;
                    case UnitStat.MaxShield:
                        stats.IncreaseMaxShieldModifier(-(currentValue ? stats.MaxShield : consumedValue));
                        return;
                    case UnitStat.CurrentShield:
                        stats.CurrentShield -= (currentValue ? stats.CurrentShield : consumedValue);
                        return;
                    default:
                        return;
                }
            }
        }
    }
}


