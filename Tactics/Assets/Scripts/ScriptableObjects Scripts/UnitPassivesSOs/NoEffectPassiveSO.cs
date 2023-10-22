using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectPassiveSO : UnitPassiveSO
    {
        [SerializeField] private bool removeOnTurnEnd = false;
        [SerializeField] private bool removeOnTurnStart = false;
        
        protected override IEnumerator OnAddedEffect(Unit unit,UnitPassiveInstance instance)
        {
            yield break;
        }

        protected override IEnumerator OnRemovedEffect(Unit unit,UnitPassiveInstance instance)
        {
            yield break;
        }
        
        protected override IEnumerator OnTurnEndEffect(Unit unit,UnitPassiveInstance instance)
        {
            instance.SetRemoveOnTurnEnd(removeOnTurnEnd);
            yield break;
        }

        protected override IEnumerator OnTurnStartEvent(Unit unit,UnitPassiveInstance instance)
        {
            instance.SetRemoveOnTurnStart(removeOnTurnStart);
            yield break;
        }
    }
}


