using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackableActionManager : MonoBehaviour
{
    [SerializeField] private bool showLog = false;

    private void Start()
    {
        StackableAction.showLog = showLog;
        StackableAction.Manager.Init(this);


        StackableAction.ActionStartInvoker<MainAction>.OnInvoked += action => Debug.Log("Action started (invoked)");
        StackableAction.ActionEndInvoker<MainAction>.OnInvoked += action => Debug.Log("Action ended (invoked)");

        var test = new MainAction(3);
        
        test.Run();
    }
    
    [ContextMenu("Advance")]
    private void Advance()
    {
        StackableAction.Manager.AdvanceAction();
    }

    private class MainAction : StackableAction
    {
        private RoundAction currentRoundAction;
        private int maxRounds;
        
        public MainAction(int maxRounds) : base()
        {
            this.maxRounds = maxRounds;
        }
        
        protected override YieldedAction MainYieldedAction()
        {
            currentRoundAction = new RoundAction(1);
            
            return new YieldedAction(RunCurrentRound);
        }

        private void RunCurrentRound()
        {
            currentRoundAction.Run();

            EnqueueNextRound();
        }

        private void EnqueueNextRound()
        {
            var roundCount = currentRoundAction.CurrentRound + 1;
            
            if(roundCount > maxRounds) return;
            
            currentRoundAction = new RoundAction(roundCount);
            
            EnqueueYieldedActions(new YieldedAction(RunCurrentRound));
        }
    }
    
    private class RoundAction : SimpleStackableAction
    {
        public int CurrentRound { get; }
        
        public RoundAction(int round) : base()
        {
            CurrentRound = round;
        }

        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        protected override void Main()
        {
            Debug.Log($"Starting round {CurrentRound}");
        }

        protected override void PostWaitAction()
        {
            
        }
    }
}

public abstract class SimpleStackableAction : StackableAction
{
    protected abstract YieldInstruction YieldInstruction { get; }                     
    protected abstract CustomYieldInstruction CustomYieldInstruction { get; }
    
    protected override YieldedAction MainYieldedAction()
    {
        if(CustomYieldInstruction != null) return new YieldedAction(Main,CustomYieldInstruction,PostWaitAction);
        return new YieldedAction(Main,YieldInstruction,PostWaitAction);
    }
    
    protected abstract void Main();
    protected abstract void PostWaitAction();
}

public abstract class StackableAction
{
    //global
    protected enum State
    {
        Created,
        Stacked,
        Starting,
        Started,
        Invoking,
        Invoked,
        Ending,
        Ended,
    }
    
    public static bool showLog = false;
    private static Stack<StackableAction> stack;
    private static int maxIterationsSafety = 100;
    private static StackableAction CurrentAction => stack.TryPeek(out var action) ? action : null;
    private static MonoBehaviour coroutineInvoker;
    
    private readonly Queue<StackableAction> subActions;
    
    private readonly Queue<YieldedAction> yieldedActions;
    
    private State currentState;
    protected State CurrentState => currentState;

    
    public virtual bool AutoAdvance => true;
    
    protected StackableAction()
    {
        currentState = State.Created;
        subActions = new Queue<StackableAction>();
        yieldedActions = new Queue<YieldedAction>();
    }

    public void Run()
    {
        if (CurrentAction == null)
        {
            Stack();
            return;
        }
        
        if(CurrentAction.CurrentState is State.Starting or State.Invoking or State.Ending)
        {
            CurrentAction.subActions.Enqueue(this);
            return;
        }
        
        Stack();
    }
    
    private void InvokeStart()
    {
        CreateGenericInstance(typeof(ActionStartInvoker<>));
    }
    
    private void InvokeEnd()
    {
        CreateGenericInstance(typeof(ActionEndInvoker<>));
    }

    private void CreateGenericInstance(Type generic)
    {
        var invokable = generic.MakeGenericType(GetType());
        Activator.CreateInstance(invokable,this);
    }

    protected void Stack()
    {
        currentState = State.Stacked;
        
        Log($"Adding {this} to stack");
        
        stack.Push(this);
    }
    
    private void Start()
    {
        Log($"Starting {this}");
        
        InvokeStart();
        
        currentState = State.Started;
    }
    
    protected void EnqueueYieldedActions(YieldedAction yieldedAction)
    {
        yieldedActions.Enqueue(yieldedAction);
    }

    private void SetupActions()
    {
        EnqueueYieldedActions(MainYieldedAction());
        
        currentState = State.Invoking;
    }
    
    protected abstract YieldedAction MainYieldedAction();

    private void InvokeActions()
    {
        if (!yieldedActions.TryDequeue(out var yieldedAction))
        {
            currentState = State.Invoked;
            return;
        }
        
        var hasInstruction = yieldedAction.YieldInstruction != null;
        var hasCustomInstruction = yieldedAction.CustomYieldInstruction != null;   
        
        //do stuff
        if (!hasInstruction && !hasCustomInstruction)
        {
            //Log("No instructions to wait for");
            
            yieldedAction.PreWaitAction?.Invoke();
            yieldedAction.PostWaitAction?.Invoke();
            
            return;
        }
        
        coroutineInvoker.StartCoroutine(RunCoroutine());
        
        return;

        IEnumerator RunCoroutine()
        {
            yieldedAction.PreWaitAction?.Invoke();

            yield return hasCustomInstruction ? yieldedAction.CustomYieldInstruction : yieldedAction.YieldInstruction;
            
            yieldedAction.PostWaitAction?.Invoke();
        }
        
    }
    
    private void End()
    {
        currentState = State.Ending;
        
        Log($"Ending {this}");
        
        InvokeEnd();
        
        currentState = State.Ended;
    }

    private bool CanInvokeSubActions()
    {
        if (CurrentAction.subActions.Count <= 0) return false;
        
        var subAction = CurrentAction.subActions.Dequeue();
                
        //Log($"Found subaction {subAction}");
                
        subAction.Stack();
        return true;
    }

    private static void Advance()
    {
        var iterations = 0; // safety
        while (iterations < maxIterationsSafety)
        {
            iterations++;
            
            Log($"Advancing ({iterations})");
            if(iterations == maxIterationsSafety-1) Debug.LogWarning("Max iterations reached");

            if (CurrentAction == null)
            {
                Debug.LogWarning("Stack is empty");
                return;
            }
            
            var state = CurrentAction.CurrentState;

            //Log($"Current state of action : {state}");

            switch (state)
            {
                case State.Created: //Should never happen
                    CurrentAction.currentState = State.Stacked;
                    break;
                case State.Stacked:
                    CurrentAction.Start();
                    break;
                case State.Starting: //Should never happen
                    CurrentAction.currentState = State.Started;
                    break;
                case State.Started:
                    if (!CurrentAction.CanInvokeSubActions()) CurrentAction.SetupActions();
                    break;
                case State.Invoking:
                    if (!CurrentAction.CanInvokeSubActions())
                    {
                        CurrentAction.InvokeActions();
                        if (!CurrentAction.AutoAdvance)
                        {
                            Log("Auto Advancing disabled");
                            return;
                        }
                    }
                    break;
                case State.Invoked:
                    if(!CurrentAction.CanInvokeSubActions()) CurrentAction.End();
                    break;
                case State.Ending: //Should never happen
                    CurrentAction.currentState = State.Ended;
                    break;
                case State.Ended:
                    if(!CurrentAction.CanInvokeSubActions()) stack.Pop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class YieldedAction
    {
        public YieldInstruction YieldInstruction { get; } = null;                      // wait for seconds
        public CustomYieldInstruction CustomYieldInstruction { get; } = null;       // waitUntil or waitWhile (or another coroutine i guess)
        public Action PreWaitAction { get; } = null;
        public Action PostWaitAction { get; } = null;

        public YieldedAction(Action action)
        {
            PreWaitAction = action;
        }
        
        public YieldedAction(Action preWaitAction, YieldInstruction yieldInstruction,Action postWaitAction = null)
        {
            PreWaitAction = preWaitAction;
            YieldInstruction = yieldInstruction;
            PostWaitAction = postWaitAction;
        }
        
        public YieldedAction(Action preWaitAction, CustomYieldInstruction yieldInstruction,Action postWaitAction = null)
        {
            PreWaitAction = preWaitAction;
            CustomYieldInstruction = yieldInstruction;
            PostWaitAction = postWaitAction;
        }
    }

    public class Manager
    {
        public static void Init(MonoBehaviour monoBehaviour,int maxIterations = 0)
        {
            if(maxIterations > 0) maxIterationsSafety = maxIterations;
            coroutineInvoker = monoBehaviour;
            stack = new Stack<StackableAction>();
        }
        
        public static void AdvanceAction()
        {
            Advance();
        }
    }
    
    public class ActionStartInvoker<T> where T : StackableAction
    {
        public static event Action<T> OnInvoked;
        
        public ActionStartInvoker(T stackableAction)
        {
            OnInvoked?.Invoke(stackableAction);
        }
    }
    
    public class ActionEndInvoker<T> where T : StackableAction
    {
        public static event Action<T> OnInvoked;
        
        public ActionEndInvoker(T stackableAction)
        {
            OnInvoked?.Invoke(stackableAction);
        }
    }
    
    private static void Log(string text)
    {
        if(!showLog) return;
        Debug.Log(text);
    }
}
