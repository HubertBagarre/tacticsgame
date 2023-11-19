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
        [SerializeField] private int maxRounds = 10;

        [Header("Managers")]
        [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private AbilityManager abilityManager;
        [SerializeField] private TimelineManager timelineManager;
        
        //Level
        private BattleLevel CurrentLevel { get; set; }
        
        //Battle
        private bool IsBattleStarted { get; set; }
        
        [ContextMenu("End Unit Turn")]
        private void EndCurrentTimelineEntityTurn()
        {
            timelineManager.CurrentTimelineEntity.EndTurn();
        }
        
        public void SetLevel(BattleLevel level)
        {
            CurrentLevel = level;
        }

        public void StartBattle()
        {
            // 0 - Setup default values (lists, dictionaries, etc)
            
            timelineManager.CreateNewTimeline();
            
            
            // 1 - Spawn Tiles
            // Already there cuz of BattleLevel
            
            tileManager.SetTiles(CurrentLevel.Tiles);
            
            // 3 - Spawn Units (timeline should be set up and they should spawn with their passives)
            
            foreach (var placedUnit in CurrentLevel.PlacedUnits)
            {
                var tile = tileManager.AllTiles.FirstOrDefault(tile => tile.Position == placedUnit.position);
                
                var unit = new NewUnit(placedUnit.so,tile);
                var unitRenderer = unitManager.SpawnUnit(unit);
            
                var unitTr = unitRenderer.transform;
                unitTr.position = tile.transform.position;
                unitTr.rotation = Quaternion.identity;
                unitTr.SetParent(transform);
                
                timelineManager.AddUnitToTimeline(unit,true);
            }
            
            timelineManager.ResetRoundEntityDistanceFromTurnStart();
            
            // 4 - Setup Win/Lose Conditions
            

            // 5 - Start Round Entity's turn (a round starts at the end of Round entity's turn and end at the end of Round entity's turn)
            
            ActionStartInvoker<RoundAction>.OnInvoked += action => Debug.Log($"Starting round {action.CurrentRound}");
            
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
            
                if(BattleManager.maxRounds > 0 && currentRoundAction.CurrentRound > BattleManager.maxRounds) return;
                
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
                var currentEntity = BattleManager.timelineManager.FirstTimelineEntity;
                
                if(currentEntity == null) return;

                var isEndRound = BattleManager.timelineManager.IsCurrentEntityEndRoundEntity;
                
                //Debug.Log($"Should end round : {isEndRound}");

                if (!isEndRound)
                {
                    EnqueueYieldedActions(new YieldedAction(Main));
                    
                    var entityTimelineAction = new TimelineEntityTurnAction(currentEntity);
                    
                    entityTimelineAction.TryStack();
                }

                BattleManager.timelineManager.AdvanceTimeline();
                
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
