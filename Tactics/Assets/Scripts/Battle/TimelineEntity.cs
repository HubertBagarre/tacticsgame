// Create a class that represents an entity on the timeline
// a timeline entity has a speed, a portrait, a team, a distance from the start of the turn and a turn order and start and end turn events
// it also has a method to reset the turn value and a method to decay the turn value

using System;
using UnityEngine;

namespace Battle
{
    public abstract class TimelineEntity : IComparable<TimelineEntity>
    {
        public string Name { get; }
        public Sprite Portrait { get; protected set; }
        public int Team { get; protected set; } = 0;
        public int Initiative { get; protected set; }
        public int Speed { get; protected set; }
        public float DistanceFromTurnStart { get; protected set; }
        public event Action<float> OnDistanceFromTurnStartChanged;
        public float TurnOrder => DistanceFromTurnStart / (Speed / 100f);
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
            JoinedIndex = value;
        }

        public void DecayTurnValue(float amount)
        {
            //Debug.Log($"Decaying {Name}'s turn value by {amount} (rate : {DecayRate})");
            DistanceFromTurnStart -= amount * DecayRate;
            OnDistanceFromTurnStartChanged?.Invoke(DistanceFromTurnStart);
        }

        public void SetDistanceFromTurnStart(float value)
        {
            DistanceFromTurnStart = value;
            OnDistanceFromTurnStartChanged?.Invoke(DistanceFromTurnStart);
        }

        public void OnTurnStart()
        {
            InTurn = true;
            //Debug.Log($"Turn Started {Name}");

            TurnStart();
        }

        protected abstract void TurnStart();

        public void OnTurnEnd()
        {
            TurnEnd();

            //Debug.Log($"Turn Ended {Name}");
        }

        protected abstract void TurnEnd();

        public void EndTurn()
        {
            InTurn = false;
            Debug.Log("Ending turn");
        }

        public int CompareTo(TimelineEntity other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var turnOrder = TurnOrder.CompareTo(other.TurnOrder);
            return turnOrder != 0 ? turnOrder : JoinedIndex.CompareTo(other.JoinedIndex);
        }
    }
}

namespace Battle.ActionSystem.TimelineActions
{
    public class TimelineEntityTurnAction : BattleAction
    {
        public TimelineEntity Entity { get; }

        public TimelineEntityTurnAction(TimelineEntity entity)
        {
            Entity = entity;
            CustomYieldInstruction = new WaitUntil(() => !Entity.InTurn);
        }

        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }

        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<TimelineEntityTurnAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<TimelineEntityTurnAction>(this));
        }

        protected override void AssignedActionPreWait()
        {
            Entity.OnTurnStart();
        }

        protected override void AssignedActionPostWait()
        {
            Entity.OnTurnEnd();
        }
    }
}