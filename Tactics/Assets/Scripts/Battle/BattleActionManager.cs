using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ActionSystem;
    
    public class BattleActionManager : MonoBehaviour
    {
        private BattleAction currentAction;

        private void Start()
        {
            StartAction();
        }

        public void StartAction()
        {
            var mainAction = new MainBattleAction(this);

            mainAction.Start();
        }
    }
    
    public class MainBattleAction : BattleAction
    {
        protected override WaitForSeconds Wait { get; }
        protected override MonoBehaviour CoroutineInvoker { get; }

        public MainBattleAction(MonoBehaviour coroutineInvoker)
        {
            CoroutineInvoker = coroutineInvoker;
            Wait = new WaitForSeconds(0.1f);
        }
        
        protected override void AssignedAction()
        {
            Debug.Log("Main action");
        }
        
        // Copy this in every BattleAction (replace MainBattleAction with the name of the class)
        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<MainBattleAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<MainBattleAction>(this));
        }
    }
}

namespace Battle.ActionSystem
{
    public abstract class BattleAction
    {
        private BattleAction Parent { get; set; }
        protected virtual MonoBehaviour CoroutineInvoker => Parent.CoroutineInvoker;

        private bool IsStarted { get; set; }
        private bool IsOver { get; set; }

        protected abstract WaitForSeconds Wait { get; }
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
            
            actionsTriggeredDuringStart.Clear();
            actionsTriggeredDuringAction.Clear();
            actionsTriggeredDuringEnd.Clear();
            
            ExecuteStep();
        }
        
        public void StartBattleAction(BattleAction battleAction)
        {
            if(battleAction.IsStarted || battleAction.IsOver) return;

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
            if(actionsTriggeredDuringStart.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringAction.Any(battleAction => !battleAction.IsOver)) return;
            if(actionsTriggeredDuringEnd.Any(battleAction => !battleAction.IsOver)) return;
            
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
                AssignedAction();
                yield return Wait;
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
            IsOver = true;
            Parent?.ResumeAction();
        }
        
        protected abstract void AssignedAction();
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


