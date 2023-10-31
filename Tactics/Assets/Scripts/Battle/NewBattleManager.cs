using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ActionSystem;
    using ActionSystem.TimelineActions;

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
        [SerializeField] private bool showLog;
        
        [Header("Managers")]
        [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private AbilityManager abilityManager;
        
        //Timeline
        [field:Header("Timeline")]
        [field: SerializeField] public int ResetTurnValue { get; private set; } = 100;
        private List<TimelineEntity> entitiesInTimeline;
        public TimelineEntity CurrentTimelineEntity { get; private set; }
        
        //Round
        private int CurrentRound  { get; set; }
        
        private bool IsBattleStarted { get; set; }
        
        private MainBattleAction mainBattleAction;
        private RoundAction CurrentRoundAction;
        private TestEntity RoundEndEntity;

        [ContextMenu("End Unit Turn")]
        private void EndUnitTurn()
        {
            CurrentTimelineEntity.EndTurn();
        }

        private void Awake()
        {
            BattleAction.showLog = showLog;
            
            mainBattleAction = new MainBattleAction(this);
        }
        
        private void Start()
        {
            // 0 - Setup default values (lists, dictionaries, etc)
            
            
            // 1 - Spawn Tiles

            entitiesInTimeline = new List<TimelineEntity>();
            
            // 3 - Spawn Units (timeline should be set up and they should spawn with their passives)
            AddEntityToTimeline(new TestEntity(100,10,"0"));
            AddEntityToTimeline(new TestEntity(100,10,"1"));
            AddEntityToTimeline(new TestEntity(100,10,"2"));

            RoundEndEntity = new TestEntity(50, 10, "End Round");
            AddEntityToTimeline(RoundEndEntity);
            
            
            // 4 - Setup Win/Lose Conditions
            
            // 5 - Start Round Entity's turn (a round starts at the end of Round entity's turn and end at the end of Round entity's turn)
            CurrentRound = 0;
            CurrentTimelineEntity = RoundEndEntity;
            
            CurrentRoundAction = new RoundAction(this);
            
            
            IsBattleStarted = true;
            
            mainBattleAction.EnqueueInActionStart(CurrentRoundAction);
            mainBattleAction.Start();
        }

        public void AddEntityToTimeline(TimelineEntity entity)
        {
            entitiesInTimeline.Add(entity);
            
            entity.ResetDistanceFromTurnStart(entity.Initiative);
        }

        public void AdvanceTimeline()
        {
            CurrentTimelineEntity?.ResetDistanceFromTurnStart(ResetTurnValue);
            
            CurrentTimelineEntity = entitiesInTimeline.OrderBy(entity => entity.TurnOrder).ToList().First();
            
            var amountToDecay = CurrentTimelineEntity.TurnOrder;
            
            /*
            Debug.Log("Time line advanced :");
            foreach (var timelineEntity in entitiesInTimeline)
            {
                Debug.Log($"{timelineEntity.Name} : {timelineEntity.DistanceFromTurnStart} (speed : {timelineEntity.Speed})");   
            }
            
            
            Debug.Log("Decaying Turn Values line advanced :");*/
            foreach (var entity in entitiesInTimeline)
            {
                entity.DecayTurnValue(amountToDecay);
            }
            
            
        }
        
        private class MainBattleAction : BattleAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            protected override MonoBehaviour CoroutineInvoker { get; }

            public MainBattleAction(NewBattleManager battleManager)
            {
                CoroutineInvoker = battleManager;
                CustomYieldInstruction = new WaitUntil(()=>battleManager.IsBattleStarted);
                SetAsCurrentRunningAction();
            }

            protected override void AssignedActionPreWait()
            {
                
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

        private class RoundAction : BattleAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            protected override MonoBehaviour CoroutineInvoker => BattleManager;
            protected NewBattleManager BattleManager { get; }
            
            public int Round => BattleManager.CurrentRound;
            
            public RoundAction(NewBattleManager battleManager)
            {
                BattleManager = battleManager;
            }
            
            protected override void StartActionEvent()
            {
                EventManager.Trigger(new StartBattleAction<RoundAction>(this));
            }

            protected override void EndActionEvent()
            {
                EventManager.Trigger(new EndBattleAction<RoundAction>(this));
            }

            protected override void AssignedActionPreWait()
            {
                BattleManager.CurrentRoundAction = this;
                
                BattleManager.AdvanceTimeline();
            }

            protected override void AssignedActionPostWait()
            {
                //Debug.Log($"CurrentTimelineEntity != Round End ? {(BattleManager.CurrentTimelineEntity != BattleManager.RoundEndEntity)}");
                if (BattleManager.CurrentTimelineEntity != BattleManager.RoundEndEntity)
                {
                    // enqueue Current Entity Turn action
                    EnqueueInActionStart(new TimelineEntityTurnAction(BattleManager.CurrentTimelineEntity));
                    
                    //Debug.Log($"Queued {BattleManager.CurrentTimelineEntity.Name}'s turn");
                    
                    // rollback to step 1 so we can start the unit action
                    SetStep(0);
                    
                    return;
                }
                
                //if Current Entity is Round End Entity, queue next round
                BattleManager.CurrentRound++;
                
                Parent.EnqueueInActionStart(new RoundAction(BattleManager));
            }
        }
        
        private class TestEntity : TimelineEntity
        {
            public TestEntity(int speed, int initiative, string name = "Test Entity") : base(speed, initiative, name)
            {
                
            }
            
            protected override void TurnStart()
            {
                Debug.Log($"Started {Name}'s turn");
            }

            protected override void TurnEnd()
            {
                
            }
        }
    }
}
// Create a class that represents an entity on the timeline
// a timeline entity has a speed, a portrait, a team, a distance from the start of the turn and a turn order and start and end turn events
// it also has a method to reset the turn value and a method to decay the turn value

namespace Battle
{
    public abstract class TimelineEntity
    {
        public string Name { get; }
        public Sprite Portrait { get; protected set; }
        public int Team { get; protected set; } = 0;
        public int Initiative { get; protected set; }
        public int Speed { get; protected set; }
        public float DistanceFromTurnStart { get; protected set; }
        public float TurnOrder => DistanceFromTurnStart / (Speed / 100f);
        public float DecayRate => Speed / 100f;
        
        public bool InTurn { get; private set; } = false;

        public TimelineEntity(int speed,int initiative, string name)
        {
            Speed = speed;
            Initiative = initiative;
            Name = name;
        }
        
        public void DecayTurnValue(float amount)
        {
            //Debug.Log($"Decaying {Name}'s turn value by {amount} (rate : {DecayRate})");
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void ResetDistanceFromTurnStart(float value)
        {
            DistanceFromTurnStart = value;
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