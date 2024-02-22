using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
	
	
	// Simple class that invokes two actions, one before waiting and one after waiting
	public class YieldedAction
	{
		public YieldInstruction YieldInstruction { get; } = null; // Usually WaitForSeconds or WaitForEndOfFrame

		public CustomYieldInstruction CustomYieldInstruction { get; } = null; // Wait Until or Wait While, or Another Coroutine, use to wait for player input, for example

		public Action PreWaitAction { get; } = null;
		public Action PostWaitAction { get; } = null;

		public YieldedAction(Action action) // When an action doesn't have a delay or a wait
		{
			PreWaitAction = action;
		}

		// You can use either a YieldInstruction or a CustomYieldInstruction, they are not derived from the same class :(
		public YieldedAction(Action preWaitAction, YieldInstruction yieldInstruction, Action postWaitAction = null)
		{
			PreWaitAction = preWaitAction;
			YieldInstruction = yieldInstruction;
			PostWaitAction = postWaitAction;
		}

		public YieldedAction(Action preWaitAction, CustomYieldInstruction yieldInstruction,
			Action postWaitAction = null)
		{
			PreWaitAction = preWaitAction;
			CustomYieldInstruction = yieldInstruction;
			PostWaitAction = postWaitAction;
		}
	}
	
	public abstract class StackableAction
	{
		private static bool showLog = false;				// Toggle Debug.Log
		private static int maxIterationsSafety;			// Safety for infinite loops (if Action doesn't have any delay, they will run in a while(true) loop)
		private static MonoBehaviour coroutineInvoker; // Only monoBehaviour can start coroutines
		
		//global
		protected enum State
		{
			Created,	// Constructed
			Stacked,	// When added to stack
			Starting,	// Call starting event here
			Started,	// Put subactions that where added during Starting on the stack
			Invoking,	// Put subactions that where added during Started on the stack, if none, invoke main action
			Invoked,	// Put subactions that where added during Invoking on the stack, if none, end
			Ending,	// Call ending event here
			Ended,		// Put subactions that where added during Ending on the stack, if none, pop from stack
		}
		protected State CurrentState { get; private set; } = State.Created;
		
		private static Stack<StackableAction> stack; // Stack of actions, the top one is the one that will advance (Advancing a Stackable action means changing its state, and reacting accordingly)
		private static StackableAction CurrentAction => stack.TryPeek(out var action) ? action : null;

		private readonly Queue<StackableAction> subActions = new(); // Actions that where added during the current action. They are pushed to the stack during certain states (see enum State)
		
		private readonly Queue<YieldedAction> yieldedActions = new(); 
		protected abstract YieldedAction MainYieldedAction(); // Mandatory, gets automatically added to the queue of yielded actions
		protected virtual bool AutoAdvance => true; // If true, the action will advance to the next state after the yielded actions are done (player input does not AutoAdvance, for example)
		public event Action<StackableAction> OnStarted; // Called when the action starts (Starting state), found out having itself as a parameter is generally useful, but it needs to be casted to the derived class
		public event Action<StackableAction> OnEnded; // Called when the action ends (Ending state)

		// The only public method, called when you want to start the action
		// Handles if the action can directly go on the stack or if it should be a SubAction of the current action
		public void TryStack()
		{
			if (CurrentAction == null)
			{
				Stack();
				return;
			}
			
			if (CurrentAction.CurrentState is State.Starting or State.Invoking or State.Ending)
			{
				CurrentAction.subActions.Enqueue(this);
				return;
			}

			Stack();
		}
		
		// Adds the action to the stack
		protected void Stack()
		{
			CurrentState = State.Stacked;

			Log($"Adding {this} to stack");

			stack.Push(this);
		}

		// Invokes Starting events
		private void Start()
		{
			Log($"Starting {this}");

			InvokeStart();

			Log($"Started {this}");

			CurrentState = State.Started;
		}
		
		// Dequeues the yielded actions and invokes them
		private void InvokeActions()
		{
			if (!yieldedActions.TryDequeue(out var yieldedAction))
			{
				CurrentState = State.Invoked;
				Advance();
				return;
			}

			Log("Invoking actions, auto advance is " + AutoAdvance);

			var hasInstruction = yieldedAction.YieldInstruction != null;
			var hasCustomInstruction = yieldedAction.CustomYieldInstruction != null;

			// If there is no instruction, just invoke the pre and post wait actions
			if (!hasInstruction && !hasCustomInstruction)
			{
				Log("No instructions to wait for");

				yieldedAction.PreWaitAction?.Invoke();
				yieldedAction.PostWaitAction?.Invoke();

				EndInvoking();
				return;
			}

			Log($"Waiting for {(hasCustomInstruction ? "custom " : "")}instruction ");

			// If there is an instruction, starts a coroutine to handle it.
			coroutineInvoker.StartCoroutine(RunCoroutine());

			return;

			IEnumerator RunCoroutine()
			{
				yieldedAction.PreWaitAction?.Invoke();

				yield return hasCustomInstruction
					? yieldedAction.CustomYieldInstruction
					: yieldedAction.YieldInstruction;

				yieldedAction.PostWaitAction?.Invoke();

				EndInvoking();
			}

			void EndInvoking()
			{
				Log("Ended invoke actions");

				if (AutoAdvance) Advance();
			}
		}

		// Invokes Ending events
		private void End()
		{
			CurrentState = State.Ending;

			Log($"Ending {this}");

			InvokeEnd();

			Log($"Ended {this}");

			CurrentState = State.Ended;
		}

		// Called when there is an opportunity to invoke subactions.
		// If there are any subactions, dequeue and put on the stack
		private bool CanInvokeSubActions()
		{
			if (CurrentAction.subActions.Count <= 0) return false;

			var subAction = CurrentAction.subActions.Dequeue();
			
			subAction.Stack();
			return true;
		}

		// Advances the action to the next state
		private static void Advance()
		{
			var iterations = 0; // safety
			
			// The while loop exits when a yielded action has a delay or when it needs to wait, if it doesn't have any, this is essentially a while(true) loop
			// It can be patched by waiting for end of frame between the PreWaitAction and the PostWaitAction of each yielded action
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
					case State.Created:
						CurrentAction.CurrentState = State.Stacked;
						break;
					case State.Stacked:
						CurrentAction.Start();
						break;
					case State.Starting:
						CurrentAction.CurrentState = State.Started;
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
						if (!CurrentAction.CanInvokeSubActions()) CurrentAction.End();
						break;
					case State.Ending:
						CurrentAction.CurrentState = State.Ended;
						break;
					case State.Ended:
						if (!CurrentAction.CanInvokeSubActions()) stack.Pop();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// If the while loop reaches the maxIterationsSafety, it will pop the stack and advance
			Debug.LogWarning($"Max iterations reached ({maxIterationsSafety}), popping stack)");
			stack.Pop();
			Advance();
		}

		#region AutoInvokeStartAndEndEvents
		
		private void InvokeStart()
		{
			OnStarted?.Invoke(this);
			CreateGenericInstance(typeof(ActionStartInvoker<>)); //This will call ActionStartInvoker<T>.OnInvoked, with T being the derived class (so no need to cast it, unlike OnStarted)
		}

		private void InvokeEnd()
		{
			OnEnded?.Invoke(this);
			CreateGenericInstance(typeof(ActionEndInvoker<>)); //This will call ActionEndInvoker<T>.OnInvoked, with T being the derived class (so no need to cast it, unlike OnStarted)
		}
		
		private void CreateGenericInstance(Type generic)
		{
			var type = GetType(); // Gets the type of the derived class;
			var invokable = generic.MakeGenericType(type); // Creates a generic type of the derived class, so ActionStartInvoker<DerivedClass> or ActionEndInvoker<DerivedClass>;
			Activator.CreateInstance(invokable, this); // Creates an instance of the generic type, this will call the constructor of the generic type, which will invoke the event;
		}

		#endregion

		#region YieldedActions
		protected void EnqueueYieldedAction(YieldedAction yieldedAction)
		{
			yieldedActions.Enqueue(yieldedAction);
		}

		protected void EnqueueYieldedAction(Action action) // Overload for when you only want to invoke an action
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

			CurrentState = State.Invoking;
		}
		#endregion

		#region Manager
		// Manages the stack, and advances the actions
		// Also sets the coroutineInvoker and maxIterationsSafety
		// It's defined here so it can use the private members of the StackableAction class
		public static class Manager
		{
			public static void Init(MonoBehaviour monoBehaviour, int maxIterations = 0)
			{
				if (maxIterations > 0) maxIterationsSafety = maxIterations;
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

		#endregion
		
		private static void Log(string text) //There is a lot of StackableActions (basically every action in the game), so this is useful to toggle Debug.Log
		{
			if (!showLog) return;
			Debug.Log(text);
		}
	}
	
	#region AutoInvokers
	/// When constructed, will invoke the event with the action as parameter and T as the type of the action
	/// They are static, so you can subscribe to the event from anywhere
	
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
	#endregion
	
	// Simple stackable action, most SimpleStackableActions actually derived from this instead of StackableAction directly
	// It only has the most basic members and methods, so using "Generate Missing Members" on Rider will not generate a lot of code
	public abstract class SimpleStackableAction : StackableAction
	{
		protected abstract YieldInstruction YieldInstruction { get; }
		protected abstract CustomYieldInstruction CustomYieldInstruction { get; }

		protected override YieldedAction MainYieldedAction()
		{
			if (CustomYieldInstruction != null) return new YieldedAction(Main, CustomYieldInstruction, PostWaitAction);
			return new YieldedAction(Main, YieldInstruction, PostWaitAction);
		}

		protected abstract void Main();

		protected virtual void PostWaitAction()
		{
		}
	}
}