// Create a class that represents an entity on the timeline
// a timeline entity has a speed, a portrait, a team, a distance from the start of the turn and a turn order and start and end turn events
// it also has a method to reset the turn value and a method to decay the turn value

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public abstract class TimelineEntity : IComparable<TimelineEntity>
    {
        public string Name { get; }
        public Sprite Portrait { get; protected set; }
        public int Team { get; protected set; } = 0;
        public int Initiative { get; protected set; }
        public int Speed { get; private set; }
        public event Action<int> OnSpeedChanged;
        public float DistanceFromTurnStart { get; private set; }
        public event Action<float> OnDistanceFromTurnStartChanged;
        public float TurnOrder => DistanceFromTurnStart * 100 / Speed;
        public int JoinedIndex { get; private set; }
        public float DecayRate => Speed / 100f;

        public bool InTurn { get; private set; } = false;

        public TimelineEntity(int speed, int initiative, string name)
        {
            Speed = speed;
            Initiative = initiative;
            Name = name;
        }

        public void SetJoinIndex(int value)
        {
            JoinedIndex = -value;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
            OnDistanceFromTurnStartChanged?.Invoke(DistanceFromTurnStart);
        }

        public void SetDistanceFromTurnStart(float value)
        {
            DistanceFromTurnStart = value;
            OnDistanceFromTurnStartChanged?.Invoke(DistanceFromTurnStart);
        }

        public virtual void SetSpeed(int value)
        {
            Speed = value;
            OnSpeedChanged?.Invoke(Speed);
        }
        
        public void OnAddedToTimeline()
        {
            AddedToTimelineEffect();
        }

        protected abstract void AddedToTimelineEffect();

        public void OnTurnStart()
        {
            InTurn = true;

            TurnStart();
        }

        protected abstract void TurnStart();

        public void OnTurnEnd()
        {
            TurnEnd();
        }

        protected abstract void TurnEnd();

        public void EndTurn()
        {
            InTurn = false;
        }

        public int CompareTo(TimelineEntity other)
        {
            var turnOrder = TurnOrder.CompareTo(other.TurnOrder);
            if (turnOrder != 0) return turnOrder;
            
            //if (JoinedIndex < 0) return 1;
            
            return JoinedIndex.CompareTo(other.JoinedIndex);
        }
        
        public abstract IEnumerable<StackableAction.YieldedAction> EntityTurnYieldedActions { get; }
    }
}

namespace Battle.ActionSystem.TimelineActions
{
    public class TimelineEntityTurnAction : SimpleStackableAction
    {
        public TimelineEntity Entity { get; }
        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        
        public TimelineEntityTurnAction(TimelineEntity entity)
        {
            Entity = entity;
        }
        
        protected override void Main()
        {
            var enumerable = Entity.EntityTurnYieldedActions;
            if (enumerable == null)
            {
                Debug.LogWarning($"No yielded actions for {Entity.Name}");
                return;
            }
                
            foreach (var behaviourAction in enumerable)
            {
                EnqueueYieldedAction(behaviourAction);
            }
        }
    }
}