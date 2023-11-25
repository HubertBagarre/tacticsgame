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
        [SerializeField] private NewUIBattleManager uiManager;
        
        //Level
        private BattleLevel CurrentLevel { get; set; }

        private Queue<UnitPlacementSO.PlacedUnit> unitsToPlace = new();
        
        //Current Unit
        private UnitTurnBattleAction CurrentTurnUnitAction { get; set; }
        
        public void SetLevel(BattleLevel level)
        {
            CurrentLevel = level;
        }

        public void StartBattle()
        {
            // 0 - Setup default values (lists, dictionaries, etc)
            AddCallbacks();
            
            // 1 - Spawn Tiles
            // Already there cuz of BattleLevel
            
            tileManager.SetTiles(CurrentLevel.Tiles);
            
            // 4 - Setup Win/Lose Conditions
            
            StackableAction.Manager.ShowLog(showLog);
            StackableAction.Manager.Init(this,maxIterations);
            
            var mainBattleAction = new MainBattleAction(this);
            
            mainBattleAction.TryStack();
            
            Advance();
        }

        private void SpawnStartingUnits(MainBattleAction action)
        {
            ActionStartInvoker<MainBattleAction>.OnInvoked -= SpawnStartingUnits;
            
            timelineManager.CreateNewTimeline();
            
            var spawnStartingUnitsAction = new SpawnStartingUnitsAction(this);
            
            spawnStartingUnitsAction.TryStack();
            
        }

        private void ResetTimelineAfterEntitySpawn(SpawnStartingUnitsAction action)
        {
            ActionEndInvoker<SpawnStartingUnitsAction>.OnInvoked -= ResetTimelineAfterEntitySpawn;
            
            timelineManager.ResetRoundEntityDistanceFromTurnStart();
        }

        private void AddCallbacks()
        {
            ActionStartInvoker<RoundAction>.OnInvoked += action => Debug.Log($"Starting round {action.CurrentRound}");
            ActionStartInvoker<MainBattleAction>.OnInvoked += SpawnStartingUnits;
            ActionEndInvoker<SpawnStartingUnitsAction>.OnInvoked += ResetTimelineAfterEntitySpawn;
            
            ActionStartInvoker<UnitTurnBattleAction>.OnInvoked += SetCurrentUnitTurnBattleAction;
            ActionEndInvoker<UnitTurnBattleAction>.OnInvoked += ClearCurrentUnitTurnBattleAction;

            ActionEndInvoker<UnitCreatedAction>.OnInvoked += AddCreatedUnitToTimeline;
            
            unitManager.AddCallbacks();
            tileManager.AddCallbacks();
            uiManager.AddCallbacks();
        }
        
        private void RemoveCallbacks()
        {
            unitManager.RemoveCallbacks();
            tileManager.RemoveCallbacks();
            uiManager.RemoveCallbacks();
            
            ActionStartInvoker<UnitTurnBattleAction>.OnInvoked -= SetCurrentUnitTurnBattleAction;
            ActionEndInvoker<UnitTurnBattleAction>.OnInvoked -= ClearCurrentUnitTurnBattleAction;
            
            ActionEndInvoker<UnitCreatedAction>.OnInvoked -= AddCreatedUnitToTimeline;
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

        private void AddCreatedUnitToTimeline(UnitCreatedAction action)
        {
            timelineManager.AddUnitToTimeline(action.Unit,true);
        }
        
        [ContextMenu("Advance")]
        private void Advance()
        {
            Debug.Log("Advancing from battleManager");
            
            StackableAction.Manager.AdvanceAction();
        }
        
        public abstract class BattleManagerAction : SimpleStackableAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            public NewBattleManager BattleManager { get; }
            
            protected BattleManagerAction(NewBattleManager battleManager)
            {
                BattleManager = battleManager;
            }
        }

        private class MainBattleAction : BattleManagerAction
        {
            private RoundAction currentRoundAction;
            
            public MainBattleAction(NewBattleManager battleManager) : base(battleManager)
            {
                currentRoundAction = new RoundAction(battleManager,1);
            }
            
            protected override void Main()
            {
                currentRoundAction.TryStack();

                currentRoundAction = new RoundAction(BattleManager,currentRoundAction.CurrentRound + 1);
            
                if(BattleManager.maxRounds > 0 && currentRoundAction.CurrentRound > BattleManager.maxRounds) return;
                
                EnqueueYieldedActions(new YieldedAction(Main));
            }
        }
        
        private class SpawnStartingUnitsAction : BattleManagerAction
        {
            public SpawnStartingUnitsAction(NewBattleManager battleManager) : base(battleManager)
            {
            }
            
            protected override void Main()
            {
                var unitsToPlace = BattleManager.CurrentLevel.PlacedUnits.Where(unit => !unit.no).ToArray();
                
                foreach (var placedUnit in unitsToPlace)
                {
                    var rendererPrefab = placedUnit.rendererPrefab;
                    var so = placedUnit.so;
                    var team = placedUnit.team;
                    var position = placedUnit.position;
                    var orientation = placedUnit.orientation;
                    var asPlayer = placedUnit.asPlayer;
                    
                    var tile = BattleManager.tileManager.AllTiles.FirstOrDefault(tile => tile.Position == position);
                
                    var unit = new NewUnit(so,tile,asPlayer);
                    unit.SetTeam(team); 
                
                    var unitCreatedAction = new UnitCreatedAction(unit,tile,SpawnPlacedUnit);
                
                    EnqueueYieldedActions(new YieldedAction(unitCreatedAction.TryStack));
                    
                    continue;
                    
                    UnitRenderer SpawnPlacedUnit()
                    {
                        return BattleManager.unitManager.SpawnUnit(unit,rendererPrefab,orientation);
                    }
                }
            }
        }
        
        public class RoundAction : BattleManagerAction
        {
            public int CurrentRound { get; }
        
            public RoundAction(NewBattleManager battleManager,int round) : base(battleManager)
            {
                CurrentRound = round;
            }
            
            protected override void Main()
            {
                var currentEntity = BattleManager.timelineManager.FirstTimelineEntity;
                var isEndTurn = BattleManager.timelineManager.IsFirstEntityRoundEnd;
                
                if(currentEntity == null) return;
                
                BattleManager.timelineManager.AdvanceTimeline();
                
                if(isEndTurn) return;
                
                EnqueueYieldedActions(new YieldedAction(Main));
                
                var entityTurnAction = new TimelineEntityTurnAction(currentEntity);
                    
                entityTurnAction.TryStack();
            }
        }

        public class UnitCreatedAction : SimpleStackableAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            
            public NewUnit Unit { get; }
            public Tile Tile { get; }
            public UnitRenderer Renderer { get; private set; }
            
            private Func<UnitRenderer> GetRenderer { get; }
            
            public UnitCreatedAction(NewUnit unit,Tile tile,Func<UnitRenderer> getRenderer)
            {
                Unit = unit;
                Tile = tile;
                GetRenderer = getRenderer;
                if(getRenderer == null) Debug.LogWarning("Get Render Method is Null");
            }
            protected override void Main()
            {
                Debug.Log($"Creating Unit {Unit.UnitSo.Name}");
                
                Renderer = GetRenderer.Invoke();
                
                var unitTr = Renderer.transform;
                unitTr.position = Tile.transform.position;
                unitTr.rotation = Quaternion.identity;
                //unitTr.SetParent(transform);
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
