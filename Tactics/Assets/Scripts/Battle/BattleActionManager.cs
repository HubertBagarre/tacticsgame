using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using ActionSystem;
    
    public class BattleActionManager : MonoBehaviour
    {
        private BattleAction currentAction;
        
        public void StartAction(BattleAction action)
        {
        
        }
    }
    
    
}

namespace Battle.ActionSystem
{
    public class BattleAction
    {
        public bool IsOver { get; protected set; }

        public Action AssignedAction { get; }
        private Stack<BattleAction> actionsTriggeredDuringThisAction;
        
        public BattleAction(Action action)
        {
            AssignedAction = action;
            IsOver = false;

            actionsTriggeredDuringThisAction = new Stack<BattleAction>();
        }

        public void Run()
        {
            AssignedAction?.Invoke();
            IsOver = true;
        }
        
        public void EnqueueAction(BattleAction action)
        {
            actionsTriggeredDuringThisAction.Push(action);
        }
    }
}


