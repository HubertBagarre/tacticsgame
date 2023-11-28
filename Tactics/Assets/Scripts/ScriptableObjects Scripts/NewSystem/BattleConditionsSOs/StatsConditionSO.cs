using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Conditions
{
    public class StatsConditionSO : UnitConditionSO
    {
        protected override IEnumerable<string> SpecificParameters =>  Enum.GetNames(typeof(UnitStat));
        
        protected override object InterpretParameter(string parameter, string value, bool defaultValue)
        {
            if (defaultValue) return 0;
            
            if(Enum.TryParse(parameter, out UnitStat stat))
            {
                switch (stat)
                {
                    case UnitStat.Hp:
                        break;
                    case UnitStat.MaxHp:
                        break;
                    case UnitStat.Movement:
                        break;
                    case UnitStat.CurrentMovement:
                        break;
                    case UnitStat.Speed:
                        break;
                    case UnitStat.Attack:
                        break;
                    case UnitStat.AttackRange:
                        break;
                    case UnitStat.MaxShield:
                        break;
                    case UnitStat.CurrentShield:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }
    }
}


