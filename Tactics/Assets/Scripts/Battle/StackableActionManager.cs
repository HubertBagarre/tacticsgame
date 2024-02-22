// This file contains the classes that manage the flow of actions in the game.
// The class YieldedAction handles time delays
// The class StackableAction handles the order and execution of YieldedActions

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
	/// <summary>
	/// Contains a yield instruction, and two actions, one to be invoked before the yield instruction, and one after
	/// Usually the yield instruction is a WaitForSeconds or WaitForEndOfFrame, and the first action launches an animation and the second one does the interesting stuff
	/// WaitForSeconds is a YieldInstruction, WaitUntil and WaitWhile are CustomYieldInstructions, they don't derive from the same class :(
	///
	/// This class will change 
	/// </summary>
	public class YieldedAction
	{
		public YieldInstruction YieldInstruction { get; } = null; // Usually WaitForSeconds or WaitForEndOfFrame

		public CustomYieldInstruction CustomYieldInstruction { get; } = null; // Wait Until or Wait While, or Another Coroutine, use to wait for player input, for example

		public Action PreWaitAction { get; } = null;
		public Action PostWaitAction { get; } = null;
		
		public bool HasCustomInstruction => CustomYieldInstruction != null;

		public YieldedAction(Action action) // When an action doesn't have a delay or a wait
		{
			PreWaitAction = action;
		}

		// You can use either a YieldInstruction or a CustomYieldInstruction
		public YieldedAction(Action preWaitAction, YieldInstruction yieldInstruction, Action postWaitAction = null)
		{
			PreWaitAction = preWaitAction;
			YieldInstruction = yieldInstruction;
			PostWaitAction = postWaitAction;
		}

		public YieldedAction(Action preWaitAction, CustomYieldInstruction yieldInstruction, Action postWaitAction = null)
		{
			PreWaitAction = preWaitAction;
			CustomYieldInstruction = yieldInstruction;
			PostWaitAction = postWaitAction;
		}

		// Coroutine that waits for the yield instruction, and invokes the pre and post wait actions (has a callback as parameter)
		public IEnumerator RunCoroutine(Action onCompleted)
		{
			PreWaitAction?.Invoke();

			yield return HasCustomInstruction ? CustomYieldInstruction : YieldInstruction;

			PostWaitAction?.Invoke();

			onCompleted?.Invoke();
		}
	}
	
	/// <summary> 
	/// Contains a queue of YieldedActions, since there can only be 1 wait per YieldedAction, you need multiple ones to handle multiple waits (for example a 3 hit strike)
	///
	/// StackableActions are put on a stack and the top one is advanced, when it ends, it pops from the stack and the next one advances
	/// A StackableAction has multiple states, advancing means changing the state, and reacting accordingly
	/// 
	/// They invoke events when they start and end, so you can subscribe to them and react accordingly
	/// These events have the derived class as a parameter, so you don't have to recast it.
	/// 
	/// If something puts a new StackableAction on the stack, it will be added to the subActions queue of the current action instead
	/// StackableActions can put their own subActions on the stack, this means that StackableActions can be nested
	/// SubActions are put on the stack during certain states (see enum State)
	/// </summary>
	public abstract class StackableAction
	{
		private static bool showLog = false;				// Toggle Debug.Log
		private static int maxIterationsSafety;			// Safety for infinite loops (if Action doesn't have any delay, they will run in a while(true) loop)
		private static MonoBehaviour coroutineInvoker; // Only MonoBehaviours can start coroutines
		
		protected enum State
		{
			Created,	// Constructed
			Stacked,	// Just added to stack, call Start event
			Starting,	
			Started,	// Put subactions that where added by the Start event on the stack
			Invoking,	// Dequeue and invoke yielded actions, if there are any subactions, put them on the sack instead
			Invoked,	// Put subactions that where added during Invoking on the stack, if none, call End event
			Ending,	
			Ended,		// Put subactions that where added by the End event on the stack, if none, pop from stack
		}
		// Any StackableAction that tries to go on the stack during a Starting, Invoking or Ending state will be put on the subActions queue instead
		
		protected State CurrentState { get; private set; } = State.Created; // Default state is Created
		
		private static Stack<StackableAction> stack;
		private static StackableAction CurrentAction => stack.TryPeek(out var action) ? action : null;
		
		private readonly Queue<StackableAction> subActions = new();
		
		private readonly Queue<YieldedAction> yieldedActions = new(); 
		protected abstract YieldedAction MainYieldedAction(); // A stackableAction has at least 1 YieldedAction
		
		protected virtual bool AutoAdvance => true; // Automatically advances to the next state after the yielded actions are done (the player turn doesn't auto advance)
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
		private void Stack()
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
		
		// Dequeues the yielded actions and starts its Coroutine
		private void InvokeActions()
		{
			if (!yieldedActions.TryDequeue(out var yieldedAction))
			{
				CurrentState = State.Invoked;
				Advance();
				return;
			}

			Log("Invoking actions, auto advance is " + AutoAdvance);
			
			coroutineInvoker.StartCoroutine(yieldedAction.RunCoroutine(EndInvoking));

			return;
			
			void EndInvoking()
			{
				Log("Ended invoke actions");

				if (AutoAdvance) Advance(); // Player turn isn't AutoAdvaning,
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

		// Advances the action to the next state, see enum State
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
					case State.Created: // StackableAction trying to stack here get added to the subActions queue instead
						CurrentAction.CurrentState = State.Stacked;
						break;
					case State.Stacked:
						CurrentAction.Start();
						break;
					case State.Starting: // StackableAction trying to stack here get added to the subActions queue instead
						CurrentAction.CurrentState = State.Started;
						break;
					case State.Started:
						if (!CurrentAction.CanInvokeSubActions()) CurrentAction.SetupYieldedActions();
						break;
					case State.Invoking: // StackableAction trying to stack here get added to the subActions queue instead
						if (!CurrentAction.CanInvokeSubActions()) // Player Actions can be added here
						{
							CurrentAction.InvokeActions();
							return;
						}
						break;
					case State.Invoked:
						if (!CurrentAction.CanInvokeSubActions()) CurrentAction.End();
						break;
					case State.Ending: // StackableAction trying to stack here get added to the subActions queue instead
						CurrentAction.CurrentState = State.Ended;
						break;
					case State.Ended:
						if (!CurrentAction.CanInvokeSubActions()) stack.Pop();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// If the while loop reaches the maxIterationsSafety, it will pop the stack and advance (it shouldn't reach this point, but it's a safety measure)
			Debug.LogWarning($"Max iterations reached ({maxIterationsSafety}),	popping stack)");
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
			CreateGenericInstance(typeof(ActionEndInvoker<>)); //This will call ActionEndInvoker<T>.OnInvoked, with T being the derived class
		}
		
		private void CreateGenericInstance(Type generic)
		{
			var type = GetType(); // Gets the type of the derived class
			var invokable = generic.MakeGenericType(type); // Creates a generic type of the derived class (so ActionStartInvoker<DerivedClass> or ActionEndInvoker<DerivedClass>)
			Activator.CreateInstance(invokable, this); // Creates an instance of the generic type, this will call the constructor of the generic type, which will invoke the event
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
		
		//There are a lot of StackableActions (basically every action in the game)
		//They can quickly fill the console with logs, so you can toggle StackableActions logs on and off
		private static void Log(string text) 
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
	// using "Generate Missing Members" on Rider generate YieldInstruction, CustomYieldInstruction and Main
	public abstract class SimpleStackableAction : StackableAction
	{
		protected abstract YieldInstruction YieldInstruction { get; }
		protected abstract CustomYieldInstruction CustomYieldInstruction { get; }

		protected override YieldedAction MainYieldedAction()
		{
			return CustomYieldInstruction != null 
				? new YieldedAction(Main, CustomYieldInstruction, PostWaitAction) 
				: new YieldedAction(Main, YieldInstruction, PostWaitAction);
		}

		protected abstract void Main();

		protected virtual void PostWaitAction() { }
	}
}