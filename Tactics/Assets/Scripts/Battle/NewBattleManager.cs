using System;
using System.Collections.Generic;
using System.Linq;
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
        [Header("Managers")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private NewAbilityManager abilityManager;
        [SerializeField] private TimelineManager timelineManager;
        
        [Header("Settings")]
        [SerializeField] private int startingAbilityPoints = 3;
        [SerializeField] private Vector3 battleStartTransitionDuration = Vector3.one;
        [SerializeField] private Vector3 roundStartTransitionDuration = Vector3.one;
        
        [Header("Debug")]
        [SerializeField] private int maxIterations = 99999;
        [SerializeField] private bool showLog;
        [SerializeField] private int maxRounds = 10;
        
        //Level
        private BattleLevel CurrentLevel { get; set; }
        
        //Current Unit
        private UnitTurnBattleAction CurrentTurnUnitAction { get; set; }
        
        public void SetLevel(BattleLevel level)
        {
            CurrentLevel = level;
        }

        public void StartBattle()
        {
            
            // 0 - Setup default values (lists, dictionaries, etc)
            inputManager.SetupCamera(Camera.main); //TODO : Change use a Camera Manager (also for ultimate animations)
            abilityManager.Setup(startingAbilityPoints);
            AddCallbacks();
            
            // 1 - Spawn Tiles
            // Already there cuz of BattleLevel
            
            tileManager.SetTiles(CurrentLevel.Tiles);
            
            // 4 - Setup Win/Lose Conditions
            
            StackableAction.Manager.ShowLog(showLog);
            StackableAction.Manager.Init(this,maxIterations);
            
            var mainBattleAction = new MainBattleAction(this,battleStartTransitionDuration);
            
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
            
            Debug.Log("Done spawning units");
            
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
            abilityManager.AddCallbacks();
        }
        
        private void RemoveCallbacks()
        {
            unitManager.RemoveCallbacks();
            tileManager.RemoveCallbacks();
            abilityManager.RemoveCallbacks();
            
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

        public class MainBattleAction : BattleManagerAction
        {
            private RoundAction currentRoundAction;
            public Vector3 BattleStartTransitionDuration { get; }
            private float BattleStartTransitionDurationFloat => BattleStartTransitionDuration.x + BattleStartTransitionDuration.y + BattleStartTransitionDuration.z;

            protected override YieldInstruction YieldInstruction => new WaitForSeconds(BattleStartTransitionDurationFloat);

            public MainBattleAction(NewBattleManager battleManager,Vector3 battleStartTransitionDuration) : base(battleManager)
            {
                currentRoundAction = new RoundAction(battleManager, 1,battleManager.roundStartTransitionDuration);
                BattleStartTransitionDuration = battleStartTransitionDuration;
            }
            
            protected override void Main()
            {
            }
            
            protected override void PostWaitAction()
            {
                currentRoundAction.TryStack();

                currentRoundAction = new RoundAction(BattleManager,currentRoundAction.CurrentRound + 1,BattleManager.roundStartTransitionDuration);
            
                if(BattleManager.maxRounds > 0 && currentRoundAction.CurrentRound > BattleManager.maxRounds) return;
                
                EnqueueYieldedAction(new YieldedAction(PostWaitAction));
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
                    
                    var tile = BattleManager.tileManager.NewTiles.FirstOrDefault(tile => tile.Position == position);
                
                    var unit = new NewUnit(so,tile,asPlayer);
                    unit.SetTeam(team); 
                
                    var unitCreatedAction = new UnitCreatedAction(unit,tile,SpawnPlacedUnit);
                
                    EnqueueYieldedAction(new YieldedAction(unitCreatedAction.TryStack));
                    
                    continue;
                    
                    UnitRenderer SpawnPlacedUnit()
                    {
                        return BattleManager.unitManager.SpawnUnit(unit,rendererPrefab,orientation);
                    }
                }
                
                BattleManager.timelineManager.AddEndRoundEntity();
            }
        }
        
        public class RoundAction : BattleManagerAction
        {
            public int CurrentRound { get; }
            public Vector3 TransitionDuration { get; }
            public float TransitionDurationFloat => TransitionDuration.x + TransitionDuration.y + TransitionDuration.z;

            protected override YieldInstruction YieldInstruction => new WaitForSeconds(TransitionDurationFloat);

            public RoundAction(NewBattleManager battleManager,int round,Vector3 transitionDuration) : base(battleManager)
            {
                CurrentRound = round;
                TransitionDuration = transitionDuration;
            }
            
            protected override void Main()
            {
                
            }

            protected override void PostWaitAction()
            {
                var currentEntity = BattleManager.timelineManager.FirstTimelineEntity;
                var isEndTurn = BattleManager.timelineManager.IsFirstEntityRoundEnd;
                
                if(currentEntity == null) return;
                
                BattleManager.timelineManager.AdvanceTimeline();
                
                if(isEndTurn) return;
                
                EnqueueYieldedAction(new YieldedAction(PostWaitAction));
                
                var entityTurnAction = new TimelineEntityTurnAction(currentEntity);
                    
                entityTurnAction.TryStack();
            }
            
            
        }

        public class UnitCreatedAction : SimpleStackableAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            
            public NewUnit Unit { get; }
            public NewTile Tile { get; }
            public UnitRenderer Renderer { get; private set; }
            
            private Func<UnitRenderer> GetRenderer { get; }
            
            public UnitCreatedAction(NewUnit unit,NewTile tile,Func<UnitRenderer> getRenderer)
            {
                Unit = unit;
                Tile = tile;
                GetRenderer = getRenderer;
                if(getRenderer == null) Debug.LogWarning("Get Render Method is Null");
            }
            protected override void Main()
            {
                Debug.Log($"Creating Unit {Unit.SO.Name}");
                
                Renderer = GetRenderer.Invoke();
                
                var unitTr = Renderer.transform;
                unitTr.position = Tile.TileRenderer.transform.position;
                unitTr.rotation = Quaternion.identity;
                //unitTr.SetParent(transform);
                
                Tile.SetUnit(Unit);
            }
        }

        [Serializable]
        private class TestEntity : TimelineEntity
        {
            public override IEnumerable<YieldedAction> EntityTurnYieldedActions => null;
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
