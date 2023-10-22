using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectPassiveSO : PassiveSO<Unit>
    {
        [SerializeField] private bool removeOnTurnEnd = false;
        [SerializeField] private bool removeOnTurnStart = false;
        
        protected override IEnumerator OnAddedEffect(Unit container, PassiveInstance<Unit> instance)
        {
            yield break;
        }

        protected override IEnumerator OnRemovedEffect(Unit container, PassiveInstance<Unit> instance)
        {
            yield break;
        }

        protected override IEnumerator OnTurnEndEffect(Unit container, PassiveInstance<Unit> instance)
        {
            instance.SetRemoveOnTurnEnd(removeOnTurnEnd);
            yield break;
        }

        protected override IEnumerator OnTurnStartEvent(Unit container, PassiveInstance<Unit> instance)
        {
            instance.SetRemoveOnTurnStart(removeOnTurnStart);
            yield break;
        }
    }
}


