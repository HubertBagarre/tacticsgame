using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectEntityPassiveSO : EntityPassiveSo<Unit>
    {
        protected override IEnumerator OnAddedEffect(Unit container, PassiveInstance<Unit> instance)
        {
            yield break;
        }

        protected override IEnumerator OnRemovedEffect(Unit container, PassiveInstance<Unit> instance)
        {
            yield break;
        }

        protected override IEnumerator OnTurnEndEffect(Unit container, EntityPassiveInstance<Unit> instance)
        {
            yield break;
        }

        protected override IEnumerator OnTurnStartEvent(Unit container, EntityPassiveInstance<Unit> instance)
        {
            yield break;
        }
    }
}


