using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    using BattleEvents;
    using UnitEvents;
    using AbilityEvents;

    public class BattleManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private AbilityManager abilityManager;

        [field: Header("Settings")]
        [field: SerializeField]
        public int ResetTurnValue { get; private set; } = 999;

        [field: SerializeField] public Sprite EndTurnImage { get; private set; }

        [SerializeField] private Vector3 battleStartTransitionDuration = Vector3.one;
        [SerializeField] private Vector3 roundStartTransitionDuration = Vector3.one;

        private float TotalBattleStartTransitionDuration => battleStartTransitionDuration.x +
                                                            battleStartTransitionDuration.y +
                                                            battleStartTransitionDuration.z;

        private float TotalRoundStartTransitionDuration => roundStartTransitionDuration.x +
                                                           roundStartTransitionDuration.y +
                                                           roundStartTransitionDuration.z;


        [field: Header("Debug")]
        [field: SerializeField]
        public int CurrentRound { get; private set; }

        public event Action<Vector3> OnStartRound;

        private BattleLevel battleLevel;
        public IBattleEntity CurrentTurnEntity { get; private set; }
        private static EndRoundEntity endRoundEntity;
        public static EndRoundEntity EndRoundEntity => endRoundEntity;
        
        private static List<IEnumerator> startRoundPassives = new();
        private static List<IEnumerator> endRoundPassives = new();
 
        private List<IBattleEntity> entitiesInBattle = new();
        public IBattleEntity[] EntitiesInBattle => entitiesInBattle.Where(entity => entity.Team >= 0).ToArray();
        private List<IBattleEntity> deadUnits = new();

        private bool endBattle;
        private bool doneSetup;

        private UpdateTurnValuesEvent updateTurnValuesEvent =>
            new(CurrentTurnEntity,entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList(), endRoundEntity);

        private int SlowestEntitySpeed => GetSlowestEntitySpeed();

        public void Start()
        {
            doneSetup = false;
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<UnitDeathEvent>(AddDeadUnitToList);

            Unit.OnDistanceFromTurnStartChanged += UpdateTurnOrderOnEntityTurnOrderUpdated;
        }

        public IEnumerator SetupBattle(BattleLevel level)
        {
            battleLevel = level;
            
            startRoundPassives.Clear();
            endRoundPassives.Clear();

            InputManager.SetupCamera(Camera.main);

            foreach (var tile in tileManager.AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
            }

            UnitBehaviourSO.SetTileManager(tileManager);
            UnitBehaviourSO.SetUnitManager(unitManager);
            UnitBehaviourSO.SetAbilityManager(abilityManager);
            UnitBehaviourSO.SetBattleManager(this);
            //DelayedBattleActionsManager.Init(this); //yield return StartCoroutine() is op

            battleLevel.SetupEndBattleConditions(this);

            tileManager.SetTiles(battleLevel.Tiles);
            unitManager.SetUnits(battleLevel.Units);

            yield return StartCoroutine(battleLevel.SetupStartingEntities());
            
            SetupStartingEntities();

            doneSetup = true;
            yield break;

            void SetupStartingEntities()
            {
                entitiesInBattle.Clear();
                deadUnits.Clear();

                var entities = battleLevel.StartingEntities;
                foreach (var battleEntity in entities)
                {
                    AddEntityToBattle(battleEntity, true);
                }

                endRoundEntity = new EndRoundEntity(this, SlowestEntitySpeed);
                AddEntityToBattle(endRoundEntity, false);
            }
        }

        private int GetSlowestEntitySpeed()
        {
            var availableEntities = entitiesInBattle.Where(entity => entity != endRoundEntity)
                .Where(entity => !entity.IsDead).Where(entity => entity.Speed != 0).ToArray();
            return !(availableEntities.Length > 0) ? 100 : availableEntities.OrderBy(entity => entity.Speed).First().Speed;
        }

        private IBattleEntity GetEndRoundEntity => endRoundEntity;

        public void StartBattle()
        {
            endBattle = false;
            CurrentRound = 0;

            StartCoroutine(StartBattleTransition());

            IEnumerator StartBattleTransition()
            {
                EventManager.Trigger(new StartBattleEvent(battleStartTransitionDuration));

                yield return new WaitForSeconds(TotalBattleStartTransitionDuration);

                CurrentTurnEntity = endRoundEntity;

                NextRound();
            }
        }

        public void WinBattle()
        {
            EndBattle(true);
        }

        public void LoseBattle()
        {
            EndBattle(false);
        }

        private void EndBattle(bool win)
        {
            Debug.Log($"Ending Battle (win : {win})");

            endBattle = true;

            EventManager.RemoveListeners<EndAbilityCastEvent>();

            EventManager.Trigger(new EndBattleEvent(win));
        }

        private void NextRound()
        {
            if (endBattle) return;
            
            endRoundEntity.SetSpeed(SlowestEntitySpeed);

            CurrentRound++;

            StartRound();
        }

        private void StartRound()
        {
            StartCoroutine(InvokeOnStartRound());

            IEnumerator InvokeOnStartRound()
            {
                OnStartRound?.Invoke(roundStartTransitionDuration);

                yield return new WaitForSeconds(TotalRoundStartTransitionDuration);

                StartCoroutine(StartRoundLogicRoutine());
            }

            IEnumerator StartRoundLogicRoutine()
            {
                foreach (var battleEntity in entitiesInBattle)
                {
                    battleEntity.PreStartRound();
                }
                
                foreach (var battleEntity in entitiesInBattle)
                {
                    yield return StartCoroutine(battleEntity.StartRound());
                }

                EventManager.Trigger(new StartRoundEvent(CurrentRound, TotalRoundStartTransitionDuration));

                if (CurrentTurnEntity != null) EndCurrentEntityTurn();
            }
        }

        private void EndRound()
        {
            StartCoroutine(EndRoundLogicRoutine());

            IEnumerator EndRoundLogicRoutine()
            {
                foreach (var VARIABLE in endRoundPassives)
                {
                    
                }
                
                foreach (var battleEntity in entitiesInBattle)
                {
                    yield return StartCoroutine(battleEntity.EndRound());
                }

                EventManager.Trigger(new RoundEndEvent(CurrentRound));

                NextRound();
            }
        }

        private void DecayTurnValues(float decayValue)
        {
            foreach (var entity in entitiesInBattle)
            {
                entity.DecayTurnValue(decayValue);
            }

            EventManager.Trigger(updateTurnValuesEvent);
        }

        private void StartEntityTurn(IBattleEntity unit)
        {
            CurrentTurnEntity = unit;
            
            CurrentTurnEntity.ResetTurnValue(ResetTurnValue);
            
            if (CurrentTurnEntity == endRoundEntity)
            {
                EndRound();

                return;
            }

            EventManager.Trigger(new StartEntityTurnEvent(CurrentTurnEntity));

            StartCoroutine(CurrentTurnEntity.StartTurn(EndCurrentEntityTurn));
        }

        public void EndCurrentEntityTurn()
        {
            StartCoroutine(EndEntityTurn());

            IEnumerator EndEntityTurn()
            {
                yield return StartCoroutine(CurrentTurnEntity.EndTurn());

                EventManager.Trigger(new EndEntityTurnEvent(CurrentTurnEntity));

                NextUnitTurn();
            }
        }

        private void NextUnitTurn()
        {
            foreach (var deadUnit in deadUnits)
            {
                RemoveEntityFromBattle(deadUnit);
            }

            deadUnits.Clear();

            if (endBattle) return;

            var nextUnit = entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList().First();

            DecayTurnValues(nextUnit.TurnOrder);

            EventManager.Trigger(updateTurnValuesEvent);

            StartEntityTurn(nextUnit);
        }

        private void AddEntityToBattle(IBattleEntity entity, bool createPreview)
        {
            StartCoroutine(InitEntityForBattle());
            return;
            
            IEnumerator InitEntityForBattle()
            {
                entity.InitEntityForBattle();
                
                entitiesInBattle.Add(entity);
                entity.ResetTurnValue(-1);
                
                EventManager.Trigger(new EntityJoinBattleEvent(entity, false));
                
                yield return StartCoroutine(entity.LateInitEntityForBattle());

                if (createPreview)
                {
                    var previewEntity = new PreviewEntity(this, entity);
                    entitiesInBattle.Add(previewEntity);

                    entity.OnDeath += RemoveAssociatedPreviewEntityFromBattle;

                    EventManager.Trigger(new EntityJoinBattleEvent(previewEntity, true));

                    void RemoveAssociatedPreviewEntityFromBattle()
                    {
                        if (!deadUnits.Contains(previewEntity)) deadUnits.Add(previewEntity);
                    }
                }
                

                EventManager.Trigger(updateTurnValuesEvent);
            }
        }

        private void UpdateTurnOrderOnEntityTurnOrderUpdated(IBattleEntity entity)
        {
            if(!doneSetup) return;
            if(!entitiesInBattle.Contains(entity)) return;
                
            EventManager.Trigger(updateTurnValuesEvent);
        }

        private void RemoveEntityFromBattle(IBattleEntity entity)
        {
            if (!entitiesInBattle.Contains(entity)) return;
            entitiesInBattle.Remove(entity);

            EventManager.Trigger(new EntityLeaveBattleEvent(entity));
        }

        private void AddDeadUnitToList(UnitDeathEvent ctx)
        {
            EventManager.Trigger(updateTurnValuesEvent);

            deadUnits.Add(ctx.Unit);
        }
        
        public static void AddStartRoundPassive(IEnumerator passive)
        {
            if(!endRoundPassives.Contains(passive)) return;
            startRoundPassives.Add(passive);
        }
        
        public static void RemoveStartRoundPassive(IEnumerator passive)
        {
            startRoundPassives.Remove(passive);
        }
        
        public static void AddEndRoundPassive(IEnumerator passive)
        {
            endRoundPassives.Add(passive);
        }
        
        public static void RemoveEndRoundPassive(IEnumerator passive)
        {
            if(!endRoundPassives.Contains(passive)) return;
            endRoundPassives.Remove(passive);
        }
    }

    public class PreviewEntity : IBattleEntity
    {
        public Sprite Portrait => associatedEntity.Portrait;
        public int Team => -associatedEntity.Team;
        public int Speed => associatedEntity.Speed;
        public float DistanceFromTurnStart => associatedEntity.DistanceFromTurnStart + bm.ResetTurnValue;
        public bool IsDead => true;
        public event Action OnDeath;

        private BattleManager bm;
        private IBattleEntity associatedEntity;

        public PreviewEntity(BattleManager battleManager, IBattleEntity entity)
        {
            bm = battleManager;
            associatedEntity = entity;
        }

        public void InitEntityForBattle()
        {
        }

        public IEnumerator LateInitEntityForBattle()
        {
            yield break;
        }

        public void KillEntityInBattle()
        {
        }

        public void ResetTurnValue(float value)
        {
        }

        public void DecayTurnValue(float amount)
        {
        }

        public void PreStartRound()
        {
            
        }

        public IEnumerator StartRound()
        {
            yield return null;
        }

        public IEnumerator EndRound()
        {
            yield return null;
        }

        public IEnumerator StartTurn(Action _)
        {
            yield return null;
        }

        public IEnumerator EndTurn()
        {
            yield return null;
        }

        public override string ToString()
        {
            return $"{associatedEntity} (Preview)";
        }
    }

    public class EndRoundEntity : IBattleEntity
    {
        public Sprite Portrait { get; }
        public int Team => -1;
        public int Speed { get; private set; }
        public float DecayRate => Speed / 100f;
        public float DistanceFromTurnStart { get; private set; }
        private float TurnResetValue => battleM.ResetTurnValue;
        public bool IsDead => false;
        private BattleManager battleM;

        public EndRoundEntity(BattleManager battleManager, int speed)
        {
            battleM = battleManager;
            Portrait = battleM.EndTurnImage;

            Speed = speed > 100 ? 100 : speed;
            
            DistanceFromTurnStart = 0;
        }

        public void InitEntityForBattle()
        {
        }

        public IEnumerator LateInitEntityForBattle()
        {
            yield break;
        }

        public void KillEntityInBattle()
        {
        }

        public event Action OnDeath;

        public void SetSpeed(int newSpeed)
        {
            Speed = newSpeed > 100 ? 100 : newSpeed;
        }

        public void ResetTurnValue(float _)
        {
            DistanceFromTurnStart = TurnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void PreStartRound()
        {
            
        }

        public IEnumerator StartRound()
        {
            yield return null;
        }

        public IEnumerator EndRound()
        {
            yield return null;
        }

        public IEnumerator StartTurn(Action _)
        {
            yield return null;
        }

        public IEnumerator EndTurn()
        {
            yield return null;
        }
    }
}

namespace Battle.BattleEvents
{
    public class StartBattleEvent
    {
        public Vector3 TransitionDuration { get; }

        public StartBattleEvent(Vector3 transitionDuration)
        {
            TransitionDuration = transitionDuration;
        }
    }

    public class EndBattleEvent
    {
        public bool Win { get; }

        public EndBattleEvent(bool win)
        {
            Win = win;
        }
    }

    public class StartRoundEvent
    {
        public int Round { get; }
        public float TransitionDuration { get; }

        public StartRoundEvent(int round, float transitionDuration)
        {
            Round = round;
            TransitionDuration = transitionDuration;
        }
    }

    public class RoundEndEvent
    {
        public int Round { get; }

        public RoundEndEvent(int round)
        {
            Round = round;
        }
    }

    public class StartEntityTurnEvent
    {
        public IBattleEntity Entity { get; }

        public StartEntityTurnEvent(IBattleEntity entity)
        {
            Entity = entity;
        }
    }

    public class EndEntityTurnEvent
    {
        public IBattleEntity Entity { get; }

        public EndEntityTurnEvent(IBattleEntity entity)
        {
            Entity = entity;
        }
    }

    public class EntityJoinBattleEvent
    {
        public IBattleEntity Entity { get; }
        public bool Preview { get; }

        public EntityJoinBattleEvent(IBattleEntity entity, bool preview)
        {
            Entity = entity;
            Preview = preview;
        }
    }

    public class EntityLeaveBattleEvent
    {
        public IBattleEntity Entity { get; }

        public EntityLeaveBattleEvent(IBattleEntity entity)
        {
            Entity = entity;
        }
    }

    public class UpdateTurnValuesEvent
    {
        public List<IBattleEntity> EntityTurnOrder { get; }
        public IBattleEntity CurrentTurnEntity { get; }
        public int RoundEndIndex { get; }

        public UpdateTurnValuesEvent(IBattleEntity currentTurnEntity,List<IBattleEntity> entityTurnOrder, IBattleEntity roundEndEntity)
        {
            CurrentTurnEntity = currentTurnEntity;
            EntityTurnOrder = entityTurnOrder;
            RoundEndIndex = entityTurnOrder.IndexOf(roundEndEntity);
        }
    }
}