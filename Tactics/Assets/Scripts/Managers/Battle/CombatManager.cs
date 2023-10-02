using System;
using System.Collections;
using UnityEngine;

namespace Battle
{
    public delegate IEnumerator UnitUnitDelegate(Unit a,Unit b);
    public delegate IEnumerator UnitAttackInstanceDelegate(Unit unit,AttackInstance attackInstance);
    
    public class CombatManager : MonoBehaviour
    {
    }
}


