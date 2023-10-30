using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ActionSystem
{
    public abstract class BattleAction
    {
        private static BattleAction CurrentRunningAction { get; set; }
        private BattleAction Parent { get; set; }
        
        protected virtual MonoBehaviour CoroutineInvoker => Parent.CoroutineInvoker;

        private bool IsStarted { get; set; }
        private bool IsOver { get; set; }

        protected abstract YieldInstruction YieldInstruction { get; }
        protected abstract CustomYieldInstruction CustomYieldInstruction { get; }
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
            CurrentRunningAction?.StartBattleAction(battleAction);
        }
        
        private void StartBattleAction(BattleAction battleAction)
        {
            if(battleAction.IsOver) return;

            battleAction.Parent = this;
            
            switch (step)
            {
                case 0:
                    actionsTriggeredDuringStart.Enqueue(battleAction);
                    break;
                case 1:
                    actionsTriggeredDuringAction.Enqueue(battleAction);
                    break;
                case 2:
                    actionsTriggeredDuringAction.Enqueue(battleAction);
                    break;
                case 3:
                    actionsTriggeredDuringEnd.Enqueue(battleAction);
                    break;
                case 4:
                    actionsTriggeredDuringEnd.Enqueue(battleAction);
                    break;
                case 5:
                    Parent?.StartBattleAction(battleAction);
                    break;
                case 6:
                    Parent?.StartBattleAction(battleAction);
                    break;
                default:
                    Parent?.StartBattleAction(battleAction);
                    break;
            }
        }

        private void ResumeAction()
        {
            // No to check if actions are over because they play one by one, so when one is over execute step will 
            // try to dequeue again and the next action will be started
            /*
            if(actionsTriggeredDuringStart.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringAction.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringEnd.Any(battleAction => !battleAction.IsOver)) return;
            */

            CurrentRunningAction = this;
            
            ExecuteStep();
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
            Debug.Log($"Started {this}",CoroutineInvoker);
            
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
            Debug.Log($"Ended {this}",CoroutineInvoker);
            
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


