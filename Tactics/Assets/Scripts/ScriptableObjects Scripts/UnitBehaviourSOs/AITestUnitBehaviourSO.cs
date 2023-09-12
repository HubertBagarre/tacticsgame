using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitBehaviour/Test")]
public class AITestUnitBehaviourSO : UnitBehaviourSO
{
    public override void InitBehaviour(Unit unit)
    {
        
    }

    public override void RunBehaviour(Unit unit)
    {
        Debug.Log("AI Behaviour");
    }
}
