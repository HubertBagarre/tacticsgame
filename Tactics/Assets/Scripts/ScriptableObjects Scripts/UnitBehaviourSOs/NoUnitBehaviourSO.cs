using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public override IEnumerable<StackableAction.YieldedAction> UnitTurnBehaviourActions(NewUnit unit,Action<StackableAction.YieldedAction> enqueueActionMethod)
        {
            return new[] { new StackableAction.YieldedAction(Log,new WaitForSeconds(3f)) };
                
            void Log()
            {
                Debug.Log($"{unit}'s behaviour running");
            }
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


