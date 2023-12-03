using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
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

    protected virtual void PostWaitAction() { }
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
    
    private static bool showLog = false;
    private static Stack<StackableAction> stack;
    private static int maxIterationsSafety;
    private static StackableAction CurrentAction => stack.TryPeek(out var action) ? action : null;
    private static MonoBehaviour coroutineInvoker;
    
    private readonly Queue<StackableAction> subActions;
    
    private readonly Queue<YieldedAction> yieldedActions;
    
    private State currentState;
    protected State CurrentState => currentState;
    protected virtual bool AutoAdvance => true;

    public event Action<StackableAction> OnStarted;
    public event Action<StackableAction> OnEnded;
    
    protected StackableAction()
    {
        currentState = State.Created;
        subActions = new Queue<StackableAction>();
        yieldedActions = new Queue<YieldedAction>();
    }

    public void TryStack()
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
        OnStarted?.Invoke(this);
        CreateGenericInstance(typeof(ActionStartInvoker<>));
    }
    
    private void InvokeEnd()
    {
        OnEnded?.Invoke(this);
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
        
        Log($"Started {this}");
        
        currentState = State.Started;
    }
    
    protected void EnqueueYieldedAction(YieldedAction yieldedAction)
    {
        yieldedActions.Enqueue(yieldedAction);
    }
    
    protected void EnqueueYieldedAction(Action action)
    {
        yieldedActions.Enqueue(new YieldedAction(action));
    }
    
    protected int GetYieldedActionsCount()
    {
        return yieldedActions.Count;
    }

    private void SetupYieldedActions()
    {
        EnqueueYieldedAction(MainYieldedAction());
        
        currentState = State.Invoking;
    }
    
    protected abstract YieldedAction MainYieldedAction();

    private void InvokeActions()
    {
        if (!yieldedActions.TryDequeue(out var yieldedAction))
        {
            currentState = State.Invoked;
            Advance();
            return;
        }
        
        Log("Invoking actions, auto advance is " + AutoAdvance);
        
        var hasInstruction = yieldedAction.YieldInstruction != null;
        var hasCustomInstruction = yieldedAction.CustomYieldInstruction != null;   
        
        //do stuff
        if (!hasInstruction && !hasCustomInstruction)
        {
            Log("No instructions to wait for");
            
            yieldedAction.PreWaitAction?.Invoke(); 
            yieldedAction.PostWaitAction?.Invoke();
            
            EndInvoking();
            return;
        }
        
        Log($"Waiting for {(hasCustomInstruction ? "custom " : "")}instruction ");
        
        coroutineInvoker.StartCoroutine(RunCoroutine());
        
        return;

        IEnumerator RunCoroutine()
        {
            yieldedAction.PreWaitAction?.Invoke();

            yield return hasCustomInstruction ? yieldedAction.CustomYieldInstruction : yieldedAction.YieldInstruction;
            
            yieldedAction.PostWaitAction?.Invoke();
            
            EndInvoking();
        }
        
        void EndInvoking()
        {
            Log("Ended invoke actions");
            
            if(AutoAdvance) Advance();
        }
        
    }
    
    private void End()
    {
        currentState = State.Ending;
        
        Log($"Ending {this}");
        
        InvokeEnd();
        
        Log($"Ended {this}");
        
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
            
            if (CurrentAction == null)
            {
                Debug.LogWarning("Stack is empty, ending ");
                return;
            }
            
            var state = CurrentAction.CurrentState;

            Log($"Current state of {CurrentAction} : {state}");

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
                    if (!CurrentAction.CanInvokeSubActions()) CurrentAction.SetupYieldedActions();
                    break;
                case State.Invoking:
                    if (!CurrentAction.CanInvokeSubActions())
                    {
                        CurrentAction.InvokeActions();
                        return;
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
        
        Debug.LogWarning($"Max iterations reached ({maxIterationsSafety}), popping stack)");
        stack.Pop();
        Advance();
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
            
            Debug.Log($"Stackable Action Manager initialized, showlog is set to : {showLog}");
        }

        public static void ShowLog(bool value)
        {
            showLog = value;
        }
        
        public static void AdvanceAction()
        {
            Advance();
        }
    }
    
    private static void Log(string text)
    {
        if(!showLog) return;
        Debug.Log(text);
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
}


