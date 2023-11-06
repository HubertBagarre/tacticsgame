using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ActionSystem
{
    public abstract class BattleAction
    {
        public static bool showLog = false;
        
        public static BattleAction CurrentRunningAction { get; private set; }
        private static Queue<BattleAction> OrphanedActions { get; } = new();
        protected BattleAction Parent { get; private set; }
        
        protected virtual MonoBehaviour CoroutineInvoker => Parent.CoroutineInvoker;

        private bool IsStarted { get; set; }
        private bool IsOver { get; set; }

        protected abstract YieldInstruction YieldInstruction { get; } // wait for seconds
        protected abstract CustomYieldInstruction CustomYieldInstruction { get; } // waitUntil or waitWhile (or another coroutine i guess)
        protected abstract void StartActionEvent();
        protected abstract void EndActionEvent();

        private readonly Queue<BattleAction> actionsTriggeredDuringStart = new(); // actions started during step 0
        private readonly Queue<BattleAction> actionsTriggeredDuringAction = new(); // actions started during step 1 or 2
        private readonly Queue<BattleAction> actionsTriggeredDuringEnd = new(); // actions started during step 3 or 4

        private int step;
        
        //steps are :
        // 0 - send start action event -> actions go in actionsTriggeredDuringStart
        // 1 - start actions started during step 0 -> actions go in actionsTriggeredDuringAction
        // 2 - invoke AssignedAction and wait duration -> actions go in actionsTriggeredDuringAction
        // 3 - start actions started during step 1 or 2 -> actions go in actionsTriggeredDuringEnd
        // 4 - send end action event -> actions go in actionsTriggeredDuringEnd
        // 5 - start actions started during step 3 or 4 -> actions go in parent
        // 6 - end this action and resume parent action -> actions go in parent

        public void Start()
        {
            if(IsStarted || IsOver) return;
            
            IsStarted = true;
            IsOver = false;
            step = 0;
            
            CurrentRunningAction = this;
            
            ExecuteStep();
        }
        
        public static void StartNewBattleAction(BattleAction battleAction)
        {
            if (CurrentRunningAction != null && CurrentRunningAction.CoroutineInvoker != null)
            {
                CurrentRunningAction.StartBattleAction(battleAction);
                return;
            }

            OrphanedActions.Enqueue(battleAction);
        }

        public void EnqueueInActionStart(BattleAction battleAction)
        {
            Log($"Enqueued {battleAction} to {this} (at start)");
            battleAction.Parent = this;
            actionsTriggeredDuringStart.Enqueue(battleAction);
        }
        
        public void EnqueueInAction(BattleAction battleAction)
        {
            Log($"Enqueued {battleAction} to {this} (at action)");
            battleAction.Parent = this;
            actionsTriggeredDuringAction.Enqueue(battleAction);
        }
        
        public void EnqueueInActionEnd(BattleAction battleAction)
        {
            Log($"Enqueued {battleAction} to {this} (at end)");
            battleAction.Parent = this;
            actionsTriggeredDuringEnd.Enqueue(battleAction);
        }
        
        public void EnqueueInParent(BattleAction battleAction)
        {
            Parent?.StartBattleAction(battleAction);
        }
        
        private void StartBattleAction(BattleAction battleAction)
        {
            if(battleAction.IsOver) return;
            
            battleAction.Parent = this;
            
            switch (step)
            {
                case 0:
                    EnqueueInActionStart(battleAction);
                    break;
                case 1:
                    EnqueueInAction(battleAction);
                    break;
                case 2:
                    EnqueueInAction(battleAction);
                    break;
                case 3:
                    EnqueueInActionEnd(battleAction);
                    break;
                case 4:
                    EnqueueInActionEnd(battleAction);
                    break;
                case 5:
                    EnqueueInParent(battleAction);
                    break;
                case 6:
                    EnqueueInParent(battleAction);
                    break;
                default:
                    EnqueueInParent(battleAction);
                    break;
            }
        }

        private void ResumeAction()
        {
            Log($"Resumed {this} (at step {step})");
            
            // No need to check if actions are over because they play one by one, so when one is over execute step will 
            // try to dequeue again and the next action will be started
            /*
            if(actionsTriggeredDuringStart.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringAction.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringEnd.Any(battleAction => !battleAction.IsOver)) return;
            */

            CurrentRunningAction = this;
            
            ExecuteStep();
        }

        protected void SetStep(int value)
        {
            step = value;
        }

        private void NextStep()
        {
            step++;
            ExecuteStep();
        }

        public void ExecuteStep()
        {
            if(!IsStarted || IsOver) return;
            
            switch (step)
            {
                case 0:
                    Step0();
                    break;
                case 1:
                    Step1();
                    break;
                case 2:
                    Step2();
                    break;
                case 3:
                    Step3();
                    break;
                case 4:
                    Step4();
                    break;
                case 5:
                    Step5();
                    break;
                case 6:
                    Step6();
                    break;
                default:
                    break;
            }
        }

        // send Start Action Event
        private void Step0()
        {
            Log($"Started {this}");
            
            StartActionEvent();

            NextStep();
        }
        
        // start actions started during step 0
        private void Step1()
        {
            if(actionsTriggeredDuringStart.Count == 0)
            {
                NextStep();
                return;
            }
            
            var action = actionsTriggeredDuringStart.Dequeue();
            action.Start();
        }
        
        // invoke AssignedAction and wait duration
        private void Step2()
        {
            CoroutineInvoker.StartCoroutine(DelayAssignedAction());
            return;

            IEnumerator DelayAssignedAction()
            {
                AssignedActionPreWait();
                
                yield return CustomYieldInstruction;
                yield return YieldInstruction;
                
                AssignedActionPostWait();
                
                NextStep();
            }
        }
        
        // start actions started during step 1 or 2
        private void Step3()
        {
            if(actionsTriggeredDuringAction.Count == 0)
            {
                NextStep();
                return;
            }
            
            var action = actionsTriggeredDuringAction.Dequeue();
            action.Start();
        }
        
        // send end action event
        private void Step4()
        {
            EndActionEvent();

            NextStep();
        }
        
        // start actions started during step 3 or 4
        private void Step5()
        {
            if(actionsTriggeredDuringEnd.Count == 0)
            {
                NextStep();
                return;
            }
            
            var action = actionsTriggeredDuringEnd.Dequeue();
            action.Start();
        }
        
        // end this action and resume parent action
        private void Step6()
        {
            Log($"Ended {this}");
            
            IsOver = true;
            CurrentRunningAction = null;
            Parent?.ResumeAction();
        }
        
        /// <summary>
        /// What does this action do ?
        /// </summary>
        protected abstract void AssignedActionPreWait();
        /// <summary>
        /// Used to reset bools and stuff
        /// </summary>
        protected abstract void AssignedActionPostWait();
        
        protected void SetAsCurrentRunningAction()
        {
            CurrentRunningAction = this;
            if(CurrentRunningAction == null) return;
            while (OrphanedActions.Count > 0)
            {
                StartNewBattleAction(OrphanedActions.Dequeue());
            }
        }

        private void Log(string text)
        {
            if(!showLog) return;

            Debug.Log(text,CoroutineInvoker);
        }
    }

    public class CustomBattleAction : BattleAction
    {
        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        public Action ActionPreWait { get; }
        public Action ActionPostWait { get; }
        
        public CustomBattleAction(Action actionPreWait,Action actionPostWait = null)
        {
            ActionPreWait = actionPreWait;
            ActionPostWait = actionPostWait;
        }
        
        public CustomBattleAction(Action actionPreWait,YieldInstruction yieldInstruction, Action actionPostWait = null)
        {
            ActionPreWait = actionPreWait;
            ActionPostWait = actionPostWait;
            YieldInstruction = yieldInstruction;
        }
        
        public CustomBattleAction(Action actionPreWait,CustomYieldInstruction customYieldInstruction, Action actionPostWait = null)
        {
            ActionPreWait = actionPreWait;
            ActionPostWait = actionPostWait;
            CustomYieldInstruction = customYieldInstruction;
        }
        
        
        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<CustomBattleAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<CustomBattleAction>(this));
        }

        protected override void AssignedActionPreWait()
        {
            ActionPreWait?.Invoke();
        }

        protected override void AssignedActionPostWait()
        {
            ActionPostWait?.Invoke();
        }
    }

    public class StartBattleAction<T> where T : BattleAction
    {
        public T BattleAction { get; }
        
        public StartBattleAction(T battleAction)
        {
            BattleAction = battleAction;
        }
    }
    
    public class EndBattleAction<T> where T : BattleAction
    {
        public T BattleAction { get; }
        
        public EndBattleAction(T battleAction)
        {
            BattleAction = battleAction;
        }
    }
}


