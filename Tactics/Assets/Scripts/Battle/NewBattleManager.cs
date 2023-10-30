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
        public static bool isBattleOver;
        
        private TimelineEntity currentTimelineEntity;

        private void Awake()
        {
            mainBattleAction = new MainBattleAction(this);
        }

        [ContextMenu("Test")]
        private void Test()
        {
            isBattleOver = true;
            currentTimelineEntity.EndTurn();
        }

        private void Start()
        {
            currentTimelineEntity = new TimelineEntity();
            
            BattleAction.StartNewBattleAction(new TimelineEntityTurnAction(currentTimelineEntity));

            isBattleOver = false;
            
            mainBattleAction.Start();
        }

        public class MainBattleAction : BattleAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            protected override MonoBehaviour CoroutineInvoker { get; }

            public MainBattleAction(MonoBehaviour battleManager)
            {
                CoroutineInvoker = battleManager;
                CustomYieldInstruction = new WaitUntil(()=>isBattleOver);
                SetAsCurrentRunningAction();
            }

            protected override void AssignedActionPreWait()
            {
                Debug.Log("Main action");
            }

            protected override void AssignedActionPostWait()
            {
                
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
        public bool InTurn { get; protected set; } = false;

        public void OnTurnStart()
        {
            InTurn = true;
            Debug.Log("Turn Started");
        }

        public void OnTurnEnd()
        {
            Debug.Log("Turn Ended");
        }

        public void EndTurn()
        {
            InTurn = false;
            Debug.Log("Ending turn");
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
            CustomYieldInstruction = new WaitUntil(()=>!Entity.InTurn);
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