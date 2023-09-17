using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.UnitEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    using BattleEvents;
    using ScriptableObjects;

    public class BattleManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private TileManager tileManager;
        [SerializeField] private UnitManager unitManager;
        
        [field: Header("Settings")]
        [field: SerializeField]
        public int ResetTurnValue { get; private set; } = 999;
        [field:SerializeField] public Sprite EndTurnImage { get; private set; }
        [SerializeField] private float turnStartTransitionDuration = 1f;

        [field: Header("Debug")]
        [field: SerializeField]
        public BattleEntity CurrentTurnEntity { get; private set; }

        private EndRoundEntity endRoundEntity;

        public event Action<float> OnStartRound; 

        [field: SerializeField] public int CurrentRound { get; private set; }

        private List<BattleEntity> entitiesInBattle = new ();
        private List<Unit> deadUnits = new List<Unit>();

        private UpdateTurnValuesEvent updateTurnValuesEvent => new (entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList(),endRoundEntity);

        public void Start()
        {
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<UnitDeathEvent>(RemoveDeadUnitFromBattle);
        }

        public void SetupBattle(Level level)
        {
            foreach (var tile in tileManager.AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
            }
            
            UnitBehaviourSO.SetTileManager(tileManager);
            UnitBehaviourSO.SetUnitManager(unitManager);
            UnitBehaviourSO.SetBattleManager(this);
            DelayedBattleActionsManager.Init(this);
            
            SetupStartingEntities();
            
            void SetupStartingEntities()
            {
                entitiesInBattle.Clear();
                deadUnits.Clear();

                endRoundEntity = new EndRoundEntity(this, 100);
                AddEntityToBattle(endRoundEntity,false);

                // TODO - add units at start of battle based on level

                foreach (var battleEntity in level.StartingEntities)
                {
                    AddEntityToBattle(battleEntity,true);
                }
            }
        }

        public void StartBattle()
        {
            Debug.Log("Starting Level");

            CurrentRound = 0;
            
            EventManager.Trigger(new StartBattleEvent());

            EventManager.AddListener<StartRoundEvent>(StartEntityTurnAtRoundStart,true);
            
            NextRound();
            
            void StartEntityTurnAtRoundStart(StartRoundEvent ctx)
            {
                NextUnitTurn();
            }
        }
        
        private void NextRound()
        {
            CurrentRound++;
            
            StartRound();
        }

        private void StartRound()
        {
            Debug.Log($"Starting Round {CurrentRound}");
            
            DelayedBattleActionsManager.PlayDelayedAction(InvokeOnStartRound(),TriggerStartRoundLogic);
            
            IEnumerator InvokeOnStartRound()
            {
                OnStartRound?.Invoke(turnStartTransitionDuration);

                yield return new WaitForSeconds(turnStartTransitionDuration);
            }
            
            void TriggerStartRoundLogic()
            {
                foreach (var battleEntity in entitiesInBattle)
                {
                    battleEntity.StartRound();
                }

                EventManager.Trigger(new StartRoundEvent(CurrentRound,turnStartTransitionDuration));
            }
        }
        
        public void EndRound()
        {
            //internal stuff
            foreach (var battleEntity in entitiesInBattle)
            {
                battleEntity.EndRound();
            }
            
            EventManager.Trigger(new RoundEndEvent(CurrentRound));

            NextRound();
        }
        
        private void DecayTurnValues(float decayValue)
        {
            foreach (var entity in entitiesInBattle)
            {
                entity.DecayTurnValue(decayValue);
            }
            
            EventManager.Trigger(updateTurnValuesEvent);
        }

        private void StartEntityTurn(BattleEntity unit)
        {
            CurrentTurnEntity = unit;
            
            if (CurrentTurnEntity == endRoundEntity)
            {
                EndRound();
                
                EndCurrentEntityTurn();
                
                return;
            }
            
            CurrentTurnEntity.StartTurn();
        }
        
        public void EndCurrentEntityTurn()
        {
            Debug.Log($"Ending {CurrentTurnEntity}'s turn ");
            
            CurrentTurnEntity.ResetTurnValue(ResetTurnValue);
            
            CurrentTurnEntity.EndTurn();
            
            EventManager.Trigger(new EndEntityTurnEvent(CurrentTurnEntity));
            
            NextUnitTurn();
        }

        private void NextUnitTurn()
        {
            var nextUnit = entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList().First();
            
            DecayTurnValues(nextUnit.TurnOrder);
            
            EventManager.Trigger(updateTurnValuesEvent);
            
            StartEntityTurn(nextUnit);
        }
        
        private void AddEntityToBattle(BattleEntity entity,bool createPreview)
        {
            entity.InitEntityForBattle();

            entitiesInBattle.Add(entity);
            entity.ResetTurnValue(-1);
            
            EventManager.Trigger(new EntityJoinBattleEvent(entity,false));
            
            if (createPreview)
            {
                var previewEntity = new PreviewEntity(this,entity);
                entitiesInBattle.Add(previewEntity);
                
                EventManager.AddListener<EntityLeaveBattleEvent>(RemoveAssociatedPreviewEntityFromBattle);
                
                EventManager.Trigger(new EntityJoinBattleEvent(previewEntity,true));

                void RemoveAssociatedPreviewEntityFromBattle(EntityLeaveBattleEvent ctx)
                {
                    if(ctx.Entity != entity) return;
                    EventManager.RemoveListener<EntityLeaveBattleEvent>(RemoveAssociatedPreviewEntityFromBattle);
                    
                    RemoveEntityFromBattle(previewEntity);
                }
            }
            
            EventManager.Trigger(updateTurnValuesEvent);
        }

        private void RemoveEntityFromBattle(BattleEntity entity)
        {
            if(!entitiesInBattle.Contains(entity)) return;
            entitiesInBattle.Remove(entity);
            
            EventManager.Trigger(new EntityLeaveBattleEvent(entity));

            if (CurrentTurnEntity == entity) EndCurrentEntityTurn();
        }

        private void RemoveDeadUnitFromBattle(UnitDeathEvent ctx)
        {
            deadUnits.Add(ctx.Unit);
        }
    }

    public class PreviewEntity : BattleEntity
    {
        public Sprite Portrait => associatedEntity.Portrait;
        public int Speed => associatedEntity.Speed;
        public float DistanceFromTurnStart => associatedEntity.DistanceFromTurnStart + tm.ResetTurnValue;

        private BattleManager tm;
        private BattleEntity associatedEntity;

        public PreviewEntity(BattleManager battleManager,BattleEntity entity)
        {
            tm = battleManager;
            associatedEntity = entity;
        }

        public void InitEntityForBattle() { }
        public void KillEntityInBattle() { }

        public void ResetTurnValue(float value) { }
        public void DecayTurnValue(float amount) { }
        public void StartRound() { }
        public void EndRound() { }
        public void StartTurn() { }
        public void EndTurn() { }

        public override string ToString()
        {
            return $"{associatedEntity} (Preview)";
        }
    }

    public class EndRoundEntity : BattleEntity
    {
        public Sprite Portrait { get; }
        public int Speed { get;}
        public float DecayRate => Speed / 100f;
        public float DistanceFromTurnStart { get; private set; }
        private float TurnResetValue => battleM.ResetTurnValue;
        private BattleManager battleM;

        public EndRoundEntity(BattleManager battleManager,int speed)
        {
            battleM = battleManager;
            Portrait = battleM.EndTurnImage;

            Speed = speed;
            DistanceFromTurnStart = TurnResetValue;
        }

        public void InitEntityForBattle() { }
        public void KillEntityInBattle(){ }

        public void ResetTurnValue(float _)
        {
            DistanceFromTurnStart = TurnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void StartRound() { }
        public void EndRound() { }

        public void StartTurn() { }

        public void EndTurn() { }
    }
}

namespace Battle.BattleEvents
{
    public class StartBattleEvent
    {
    }

    public class EndBattleEvent
    {
    }

    public class StartRoundEvent
    {
        public int Round { get; }
        public float TransitionDuration { get; }

        public StartRoundEvent(int round,float transitionDuration)
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
        public BattleEntity Entity { get; }

        public StartEntityTurnEvent(BattleEntity unit)
        {
            Entity = unit;
        }
    }

    public class EndEntityTurnEvent
    {
        public BattleEntity Entity { get; }

        public EndEntityTurnEvent(BattleEntity entity)
        {
            Entity = entity;
        }
    }

    public class EntityJoinBattleEvent
    {
        public BattleEntity Entity { get; }
        public bool Preview { get; }
        
        public EntityJoinBattleEvent(BattleEntity entity,bool preview)
        {
            Entity = entity;
            Preview = preview;
        }
    }
    
    public class EntityLeaveBattleEvent
    {
        public BattleEntity Entity { get; }
        
        public EntityLeaveBattleEvent(BattleEntity entity)
        {
            Entity = entity;
        }
    }

    public class UpdateTurnValuesEvent
    {
        public List<BattleEntity> EntityTurnOrder { get; }
        public int RoundEndIndex { get; }

        public UpdateTurnValuesEvent(List<BattleEntity> entityTurnOrder, BattleEntity roundEndEntity)
        {
            EntityTurnOrder = entityTurnOrder;
            RoundEndIndex = entityTurnOrder.IndexOf(roundEndEntity);
        }
    }
}