using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectPassiveSO : UnitPassiveSO
    {
        protected override IEnumerator OnAddedEffect(Unit unit)
        {
            Debug.Log("Added Effect");
            yield return null;
        }

        protected override IEnumerator OnRemovedEffect(Unit unit)
        {
            Debug.Log("Removed Effect");
            yield return null;
        }

        protected override IEnumerator OnTurnEndEffect(Unit unit)
        {
            yield return null;
        }

        protected override IEnumerator OnTurnStartEvent(Unit unit)
        {
            yield return null;
        }
    }
}


