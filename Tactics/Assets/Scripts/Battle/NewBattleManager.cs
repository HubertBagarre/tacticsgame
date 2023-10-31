using System;
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

        [Header("Managers")] [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private AbilityManager abilityManager;

        //Timeline
        [field: Header("Timeline")]
        [field: SerializeField]
        public int ResetTurnValue { get; private set; } = 100;

        private List<TimelineEntity> entitiesInTimeline;
        public TimelineEntity CurrentTimelineEntity { get; private set; }
        public TimelineEntity FirstTimelineEntity => entitiesInTimeline.First();
        private int totalEntityAddedToTimeline = 0;

        //Round
        private int CurrentRound { get; set; }

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


            entitiesInTimeline = new List<TimelineEntity>();
            totalEntityAddedToTimeline = 0;
            
            // 1 - Spawn Tiles


            // 3 - Spawn Units (timeline should be set up and they should spawn with their passives)
            var entity0 = new TestEntity(100, 10, "0");
            var entity1 = new TestEntity(100, 11, "1");
            var entity2 = new TestEntity(100, 12, "2");
            
            AddEntitiesToTimeline(true,new List<TimelineEntity>{entity0,entity1,entity2});

            RoundEndEntity = new TestEntity(50, 10, "End Round");
            AddEntityToTimeline(RoundEndEntity,false);


            // 4 - Setup Win/Lose Conditions

            // 5 - Start Round Entity's turn (a round starts at the end of Round entity's turn and end at the end of Round entity's turn)
            CurrentRound = 0;
            CurrentTimelineEntity = RoundEndEntity;

            CurrentRoundAction = new RoundAction(this);


            IsBattleStarted = true;

            mainBattleAction.EnqueueInActionStart(CurrentRoundAction);
            mainBattleAction.Start();
        }

        public void AddEntitiesToTimeline(bool useInitiative,List<TimelineEntity> entities)
        {
            foreach (var entityToAdd in entities)
            {
                AddEntityToTimeline(entityToAdd, useInitiative);
            }
        }

        public void AddEntityToTimeline(TimelineEntity entityToAdd,bool useInitiative)
        {
            InsertEntityInTimeline(entityToAdd, useInitiative ? entityToAdd.Initiative : ResetTurnValue);
        }

        public void InsertEntityInTimeline(TimelineEntity entityToInsert,float distanceFromTurnStart)
        {
            entityToInsert.SetJoinIndex(totalEntityAddedToTimeline);
            totalEntityAddedToTimeline++;
            
            entityToInsert.SetDistanceFromTurnStart(distanceFromTurnStart);
            
            entitiesInTimeline.Add(entityToInsert);
            
            ReorderTimeline();
        }

        public void ReorderTimeline()
        {
            entitiesInTimeline.Sort();
            
            EventManager.Trigger(entitiesInTimeline);
        }

        public void ResetTimelineEntityDistanceFromTurnStart(TimelineEntity timelineEntity)
        {
            SetTimelineEntityDistanceFromTurnStart(timelineEntity,ResetTurnValue);
        }

        public void SetTimelineEntityDistanceFromTurnStart(TimelineEntity timelineEntity,float value)
        {
            timelineEntity?.SetDistanceFromTurnStart(value);
        }
        
        public void DecayEntitiesTurnValues(TimelineEntity timelineEntity,float amount)
        {
            timelineEntity.DecayTurnValue(amount);
        }

        public void AdvanceTimeline()
        {
            //Debug.Log("Advancing Timeline");
            
            //Debug.Log($"Resetting CurrentTimelineEntity's ({CurrentTimelineEntity.Name}) turn value (to {ResetTurnValue})");
            ResetTimelineEntityDistanceFromTurnStart(CurrentTimelineEntity);
            //Debug.Log($"{CurrentTimelineEntity.Name}'s turn value is now {ResetTurnValue}");
            
            //Debug.Log($"Reordering Timeline");
            ReorderTimeline();

            //Debug.Log($"Setting {FirstTimelineEntity.Name} to CurrentTimelineEntity");
            CurrentTimelineEntity = FirstTimelineEntity;

            //Debug.Log($"Decaying all entities' turn values by {CurrentTimelineEntity.TurnOrder}");
            var amountToDecay = CurrentTimelineEntity.TurnOrder;
            
            foreach (var entity in entitiesInTimeline)
            {
                DecayEntitiesTurnValues(entity, amountToDecay);
            }
            //Debug.Log($"Done advancing timeline");
        }

        private class MainBattleAction : BattleAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            protected override MonoBehaviour CoroutineInvoker { get; }

            public MainBattleAction(NewBattleManager battleManager)
            {
                CoroutineInvoker = battleManager;
                CustomYieldInstruction = new WaitUntil(() => battleManager.IsBattleStarted);
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
        
        public class RoundAction : BattleAction
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
                Debug.Log($"CurrentTimelineEntity != Round End ? {(BattleManager.CurrentTimelineEntity != BattleManager.RoundEndEntity)}");
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
