using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class CombatManager : MonoBehaviour
    {
        public delegate IEnumerator UnitAttackDelegate(Unit a,Unit b);
        
        public event UnitAttackDelegate OnUnitAttacked;
        public event UnitAttackDelegate OnUnitAttack;

        public IEnumerator UnitAttack(Unit attackingUnit,Unit attackedUnit,IEnumerator<Unit> effect)
        {
            var unitAttackRoutine = OnUnitAttack;
            if (unitAttackRoutine != null) yield return StartCoroutine(unitAttackRoutine(attackingUnit,attackedUnit));
            
            if(attackingUnit.IsDead) yield break; //TODO maybe add an inactive/incapacitated bool
            
            var unitAttackedRoutine = OnUnitAttacked;
            if (unitAttackedRoutine != null) yield return StartCoroutine(unitAttackedRoutine(attackingUnit,attackedUnit));
            
            
        }
    }
}


