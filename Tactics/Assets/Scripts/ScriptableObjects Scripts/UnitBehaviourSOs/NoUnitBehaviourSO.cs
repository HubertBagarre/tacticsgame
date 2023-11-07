using System.Collections;
using System.Collections.Generic;
using Battle.ScriptableObjects;
using UnityEngine;

namespace Battle.ScriptableObjects.UnitBehaviour
{
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/Unique/NoBehaviour")]
    public class NoUnitBehaviourSO : UnitBehaviourSO
    {
        protected override void InitBehaviourEffect(Unit unit)
        {
            
        }

        public override void ShowBehaviourPreview()
        {
            
        }

        protected override void UnitTurnBattleAction(NewUnit unit)
        {
            
        }

        protected override IEnumerator RunBehaviourEffect()
        {
            yield return null;
        }

        protected override void OnBehaviourInterruptedEffect()
        {
            
        }
    }
}


