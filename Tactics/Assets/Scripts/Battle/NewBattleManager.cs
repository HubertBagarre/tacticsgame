using System;
using System.Collections.Generic;
using System.Linq;
using Battle.ScriptableObjects;
using UnityEngine;

namespace Battle
{
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
        [SerializeField] private int maxIterations = 99999;

        [Header("Managers")]
        [SerializeField] private TileManager tileManager;
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

        //Level
        private BattleLevel CurrentLevel { get; set; }
        
        //Battle
        private bool IsBattleStarted { get; set; }
        
        private TestEntity RoundEndEntity;

        [ContextMenu("End Unit Turn")]
        private void EndCurrentTimelineEntityTurn()
        {
            CurrentTimelineEntity.EndTurn();
        }
        
        public void SetLevel(BattleLevel level)
        {
            CurrentLevel = level;
        }

        public void StartBattle()
        {
            // 0 - Setup default values (lists, dictionaries, etc)
            entitiesInTimeline = new List<TimelineEntity>();
            totalEntityAddedToTimeline = 0;
            // 1 - Spawn Tiles
            // Already there cuz of BattleLevel
            
            tileManager.SetTiles(CurrentLevel.Tiles);
            
            // 3 - Spawn Units (timeline should be set up and they should spawn with their passives)
            foreach (var placedUnit in CurrentLevel.PlacedUnits)
            {
                AddUnitToTimeline(placedUnit,true);
            }
            
            RoundEndEntity = new TestEntity(50, 10, "End Round");
            AddEntityToTimeline(RoundEndEntity, false);
            ResetRoundEntityDistanceFromTurnStart();
            
            // 4 - Setup Win/Lose Conditions
            

            // 5 - Start Round Entity's turn (a round starts at the end of Round entity's turn and end at the end of Round entity's turn)
            
            ActionStartInvoker<RoundAction>.OnInvoked += action => Debug.Log($"Starting round {action.CurrentRound}");
            
            CurrentTimelineEntity = RoundEndEntity;
            
            StackableAction.Manager.ShowLog(showLog);
            StackableAction.Manager.Init(this,maxIterations);
            
            var mainBattleAction = new MainBattleAction(this);

            IsBattleStarted = true;

            mainBattleAction.TryStack();
            
            Advance();
        }
        
        [ContextMenu("Advance")]
        private void Advance()
        {
            StackableAction.Manager.AdvanceAction();
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
        
        public void AddUnitToTimeline(UnitPlacementSO.PlacedUnit placedUnit,bool useInitiative)
        {
            var tile = tileManager.AllTiles.FirstOrDefault(tile => tile.Position == placedUnit.position);
                
            var unit = new NewUnit(placedUnit.so,tile);
            var unitRenderer = unitManager.SpawnUnit(unit);
            
            var unitTr = unitRenderer.transform;
            unitTr.position = tile.transform.position;
            unitTr.rotation = Quaternion.identity;
            unitTr.SetParent(transform);
            
            AddEntityToTimeline(unit, useInitiative);
        }

        public void InsertEntityInTimeline(TimelineEntity entityToInsert,float distanceFromTurnStart)
        {
            entityToInsert.SetJoinIndex(totalEntityAddedToTimeline);
            totalEntityAddedToTimeline++;
            if (entityToInsert == RoundEndEntity)
            {
                entityToInsert.SetJoinIndex(-1);
                totalEntityAddedToTimeline--;
            }
            
            entityToInsert.SetDistanceFromTurnStart(distanceFromTurnStart);
            
            entitiesInTimeline.Add(entityToInsert);
            
            ReorderTimeline();
            
            entityToInsert.OnAddedToTimeline();
        }

        [ContextMenu("Reorder Timeline")]
        public void ReorderTimeline()
        {
            entitiesInTimeline = entitiesInTimeline.OrderBy(entity => entity).ToList();

            for (int i = 1; i < entitiesInTimeline.Count; i++)
            {
                //Debug.Log(entitiesInTimeline[i-1].CompareTo(entitiesInTimeline[i]));
            }
            
            EventManager.Trigger(entitiesInTimeline);
        }

        public void ResetTimelineEntityDistanceFromTurnStart(TimelineEntity timelineEntity)
        {
            if (timelineEntity == RoundEndEntity)
            {
                ResetRoundEntityDistanceFromTurnStart();
                return;
            }
            SetTimelineEntityDistanceFromTurnStart(timelineEntity,ResetTurnValue);
        }
        
        public void ResetRoundEntityDistanceFromTurnStart()
        {
            var distance = RoundEndEntity.DistanceFromTurnStart;
            var speed = RoundEndEntity.Speed;
            if (entitiesInTimeline.Count > 1)
            {
                ReorderTimeline();
                
                var slowestEntity = entitiesInTimeline.Where(entity => entity != RoundEndEntity)
                    .OrderBy(entity => entity.Speed).First();
                speed = slowestEntity.Speed;
                
                var furthestEntity = entitiesInTimeline.Where(entity => entity != RoundEndEntity)
                    .OrderBy(entity => entity.TurnOrder).Last();
                distance = furthestEntity.DistanceFromTurnStart + 0.01f; // TODO : find a better way to do this (+0.01f should not be necessary)
                
            }
            
            RoundEndEntity.SetSpeed(speed);
            RoundEndEntity.SetDistanceFromTurnStart(distance);
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
            ResetTimelineEntityDistanceFromTurnStart(CurrentTimelineEntity);
            
            ReorderTimeline();

            CurrentTimelineEntity = FirstTimelineEntity;

            var amountToDecay = CurrentTimelineEntity.TurnOrder;
            
            foreach (var entity in entitiesInTimeline)
            {
                DecayEntitiesTurnValues(entity, amountToDecay);
            }
        }

        private class MainBattleAction : SimpleStackableAction
        {
            private RoundAction currentRoundAction;
            private NewBattleManager BattleManager { get; }

            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            
            public MainBattleAction(NewBattleManager battleManager)
            {
                currentRoundAction = new RoundAction(battleManager,1);
                BattleManager = battleManager;
            }
            
            protected override void Main()
            {
                currentRoundAction.TryStack();

                currentRoundAction = new RoundAction(BattleManager,currentRoundAction.CurrentRound + 1);
            
                EnqueueYieldedActions(new YieldedAction(Main));
            }
        }
        
        public class RoundAction : SimpleStackableAction
        {
            public int CurrentRound { get; }
            public TimelineEntityTurnAction CurrentEntityTurnAction { get; private set; }
            private NewBattleManager BattleManager { get; }
        
            public RoundAction(NewBattleManager battleManager,int round) : base()
            {
                CurrentRound = round;
                BattleManager = battleManager;
            }

            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            protected override void Main()
            {
                var currentEntity = BattleManager.FirstTimelineEntity;

                var isEndRound = currentEntity == BattleManager.RoundEndEntity;
                
                //Debug.Log($"Should end round : {isEndRound}");

                if (!isEndRound)
                {
                    EnqueueYieldedActions(new YieldedAction(Main));
                    
                    var entityTimelineAction = new TimelineEntityTurnAction(currentEntity);
                    
                    entityTimelineAction.TryStack();
                }

                BattleManager.AdvanceTimeline();
                
            }
        }

        [Serializable]
        private class TestEntity : TimelineEntity
        {
            public override IEnumerable<StackableAction.YieldedAction> EntityTurnYieldedActions => null;
            public TestEntity(int speed, int initiative, string name = "Test Entity") : base(speed, initiative, name)
            {
            }

            protected override void AddedToTimelineEffect()
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
