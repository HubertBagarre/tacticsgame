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
        
        //Current Unit
        private UnitTurnBattleAction CurrentTurnUnitAction { get; set; }
        
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

            AddCallbacks();
            
            StackableAction.Manager.ShowLog(showLog);
            StackableAction.Manager.Init(this,maxIterations);
            
            var mainBattleAction = new MainBattleAction(this);

            IsBattleStarted = true;

            mainBattleAction.TryStack();
            
            Advance();
        }

        private void AddCallbacks()
        {
            ActionStartInvoker<RoundAction>.OnInvoked += action => Debug.Log($"Starting round {action.CurrentRound}");
            
            ActionStartInvoker<UnitTurnBattleAction>.OnInvoked += SetCurrentUnitTurnBattleAction;
            ActionEndInvoker<UnitTurnBattleAction>.OnInvoked += ClearCurrentUnitTurnBattleAction;
        }
        
        private void RemoveCallbacks()
        {
            ActionStartInvoker<UnitTurnBattleAction>.OnInvoked -= SetCurrentUnitTurnBattleAction;
            ActionEndInvoker<UnitTurnBattleAction>.OnInvoked -= ClearCurrentUnitTurnBattleAction;
        }
        
        private void SetCurrentUnitTurnBattleAction(UnitTurnBattleAction action)
        {
            CurrentTurnUnitAction = action;
            
            CurrentTurnUnitAction.OnRequestEndTurn += Advance;
        }

        private void ClearCurrentUnitTurnBattleAction(UnitTurnBattleAction _)
        {
            if(CurrentTurnUnitAction != null) CurrentTurnUnitAction.OnRequestEndTurn -= Advance;
            
            CurrentTurnUnitAction = null;
        }
        
        [ContextMenu("Advance")]
        private void Advance()
        {
            Debug.Log("Advancing from battleManager");
            
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
                var isEndTurn = BattleManager.timelineManager.IsFirstEntityRoundEnd;
                
                if(currentEntity == null) return;
                
                //Debug.Log($"Starting entity Round : {currentEntity.Name}");
                
                BattleManager.timelineManager.AdvanceTimeline();
                
                //Debug.Log($"Next entity Round should be : {BattleManager.timelineManager.FirstTimelineEntity.Name}");
                
                if(isEndTurn) return;
                
                EnqueueYieldedActions(new YieldedAction(Main));
                
                var entityTurnAction = new TimelineEntityTurnAction(currentEntity);
                    
                entityTurnAction.TryStack();
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
