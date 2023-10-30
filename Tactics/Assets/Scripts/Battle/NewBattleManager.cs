using System.Collections.Generic;
using Battle.ActionSystem.TimelineActions;
using UnityEngine;

namespace Battle
{
    using ActionSystem;

    /// <summary>
    /// Starts battle and handles turns (TIME LINE)
    ///
    /// Battle order of operation are :
    /// 0 - Setup default values (lists, dictionaries, etc)
    /// 1 - Spawn Tiles
    /// 2 - Spawn Round Entity (timeline should be set up so it should show in preview)
    /// 3 - Spawn Units (timeline should be set up and they should spawn with their passives)
    /// 4 - Setup Win/Lose Conditions
    /// 5 - Start round 1 ( so 2nd half of Round Entity's turn, then play units turns according to timeline)
    /// </summary>
    public class NewBattleManager : MonoBehaviour
    {
        private List<TimelineEntity> timelineEntities;
        private MainBattleAction mainBattleAction;

        private void Awake()
        {
            mainBattleAction = new MainBattleAction(this);
        }

        private void Start()
        {
            var test = new TimelineEntity();
            
            BattleAction.StartNewBattleAction(new StartTurnAction(test));
            BattleAction.StartNewBattleAction(new EndTurnAction(test));
            
            mainBattleAction.Start();
        }

        public class MainBattleAction : BattleAction
        {
            protected override WaitForSeconds Wait { get; }
            protected override MonoBehaviour CoroutineInvoker { get; }

            public MainBattleAction(MonoBehaviour coroutineInvoker)
            {
                CoroutineInvoker = coroutineInvoker;
                Wait = new WaitForSeconds(0.1f);
                SetAsCurrentRunningAction();
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
}
// Create a class that represents an entity on the timeline
// a timeline entity has a speed, a portrait, a team, a distance from the start of the turn and a turn order and start and end turn events
// it also has a method to reset the turn value and a method to decay the turn value

namespace Battle
{
    public class TimelineEntity
    {
        public Sprite Portrait { get; protected set; }
        public int Team { get; protected set; }

        public int Speed { get; protected set; }
        public float DistanceFromTurnStart { get; protected set; }
        public float TurnOrder => DistanceFromTurnStart / (Speed / 100f);

        public void StartTurn()
        {
            Debug.Log("Turn Started");
        }

        public void EndTurn()
        {
            Debug.Log("Turn Ended");
        }
    }
}

namespace Battle.ActionSystem.TimelineActions
{
    public class StartTurnAction : BattleAction
    {
        public TimelineEntity Entity { get; }

        public StartTurnAction(TimelineEntity entity)
        {
            Entity = entity;
        }

        protected override WaitForSeconds Wait { get; } = new WaitForSeconds(0f);

        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<StartTurnAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<StartTurnAction>(this));
        }

        protected override void AssignedAction()
        {
            Entity.StartTurn();
        }
    }

    public class EndTurnAction : BattleAction
    {
        public TimelineEntity Entity { get; }

        public EndTurnAction(TimelineEntity entity)
        {
            Entity = entity;
        }

        protected override WaitForSeconds Wait { get; } = new WaitForSeconds(0f);

        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<EndTurnAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<EndTurnAction>(this));
        }

        protected override void AssignedAction()
        {
            Entity.EndTurn();
        }
    }
}