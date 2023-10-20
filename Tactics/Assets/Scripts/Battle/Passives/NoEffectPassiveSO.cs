using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectPassiveSO : UnitPassiveSO
    {
        protected override IEnumerator OnAddedEffect(Unit unit,UnitPassiveInstance instance)
        {
            Debug.Log("Added Effect");
            
            unit.Stats.IncreaseMaxHpModifier(10);
            
            yield return null;
        }

        protected override IEnumerator OnRemovedEffect(Unit unit,UnitPassiveInstance instance)
        {
            Debug.Log("Removed Effect");
            
            unit.Stats.IncreaseMaxHpModifier(-10);
            
            yield return null;
        }
        
        protected override IEnumerator OnTurnEndEffect(Unit unit,UnitPassiveInstance instance)
        {
            instance.SetRemoveOnTurnEnd(true);
            yield return null;
        }

        protected override IEnumerator OnTurnStartEvent(Unit unit,UnitPassiveInstance instance)
        {
            yield return null;
        }
    }
}


