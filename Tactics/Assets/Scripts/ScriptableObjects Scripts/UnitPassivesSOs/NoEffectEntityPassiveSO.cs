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

        protected override IEnumerator OnUnitTurnEndEffect(Unit battleEntity)
        {
            yield break;
        }

        protected override IEnumerator OnUnitTurnStartEffect(Unit battleEntity)
        {
            yield break;
        }
    }
}


