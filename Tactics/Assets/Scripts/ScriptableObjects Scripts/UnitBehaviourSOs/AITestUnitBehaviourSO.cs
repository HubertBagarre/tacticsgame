using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.ActionSystem;
using UnityEngine;

namespace Battle.ScriptableObjects
{

    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/Test")]
    public class AITestUnitBehaviourSO : UnitBehaviourSO
    {
        protected override void InitBehaviourEffect(Unit unit)
        {
            
        }

        public override void ShowBehaviourPreview()
        {
            
        }
        
        public override IEnumerable<StackableAction.YieldedAction> UnitTurnBehaviourActions(NewUnit unit,Action<StackableAction.YieldedAction> enqueueActionMethod)
        {
            return new[] { new StackableAction.YieldedAction(Log,new WaitForSeconds(1f)) };
                
            void Void(){}
            
            void Log()
            {
                Debug.Log($"{unit}'s behaviour running");
            }
        }

        

        protected override IEnumerator RunBehaviourEffect()
        {
            var path = AssociatedUnit.Tile.GetAdjacentTiles().ToList();
            
            AssociatedUnit.SetMovement(path.Count);
            
            yield return AssociatedUnit.StartCoroutine(AssociatedUnit.MoveUnit(path,false));
        }

        protected override void OnBehaviourInterruptedEffect()
        {
            
        }
    }
}