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
        private List<BattleEntity> deadUnits = new ();

        private UpdateTurnValuesEvent updateTurnValuesEvent => new (entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList(),endRoundEntity);

        public void Start()
        {
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<UnitDeathEvent>(AddDeadUnitToList);
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
            //DelayedBattleActionsManager.Init(this); //yield return StartCoroutine() is op
            
            SetupStartingEntities();
            
            void SetupStartingEntities()
            {
                entitiesInBattle.Clear();
                deadUnits.Clear();

                endRoundEntity = new EndRoundEntity(this, 100);
                AddEntityToBattle(endRoundEntity,false);
                
                foreach (var battleEntity in level.StartingEntities)
                {
                    AddEntityToBattle(battleEntity,true);
                }
            }

            void SetupWinCondition()
            {
                
            }

            void SetupLoseCondition()
            {
                
            }
        }

        public void StartBattle()
        {
            Debug.Log("Starting Level");

            CurrentRound = 0;
            
            EventManager.Trigger(new StartBattleEvent());

            CurrentTurnEntity = endRoundEntity;
            
            NextRound();
        }
        
        private void NextRound()
        {
            CurrentRound++;
            
            StartRound();
        }

        private void StartRound()
        {
            StartCoroutine(InvokeOnStartRound());
            
            IEnumerator InvokeOnStartRound()
            {
                OnStartRound?.Invoke(turnStartTransitionDuration);

                yield return new WaitForSeconds(turnStartTransitionDuration);
                
                StartCoroutine(StartRoundLogicRoutine());
            }
            
            IEnumerator StartRoundLogicRoutine()
            {
                foreach (var battleEntity in entitiesInBattle)
                {
                    yield return StartCoroutine(battleEntity.StartRound());
                }

                EventManager.Trigger(new StartRoundEvent(CurrentRound,turnStartTransitionDuration));
                
                if(CurrentTurnEntity != null) EndCurrentEntityTurn();
            }
        }
        
        private void EndRound()
        {
            StartCoroutine(EndRoundLogicRoutine());
            
            IEnumerator EndRoundLogicRoutine()
            {
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

        private void StartEntityTurn(BattleEntity unit)
        {
            CurrentTurnEntity = unit;
            
            if (CurrentTurnEntity == endRoundEntity)
            {
                EndRound();
                
                return;
            }
            
            EventManager.Trigger(new StartEntityTurnEvent(CurrentTurnEntity));
            
            StartCoroutine(CurrentTurnEntity.StartTurn());
        }
        
        public void EndCurrentEntityTurn()
        {
            CurrentTurnEntity.ResetTurnValue(ResetTurnValue);
            
            StartCoroutine(CurrentTurnEntity.EndTurn());
            
            EventManager.Trigger(new EndEntityTurnEvent(CurrentTurnEntity));
            
            NextUnitTurn();
        }

        private void NextUnitTurn()
        {
            foreach (var deadUnit in deadUnits)
            {
                RemoveEntityFromBattle(deadUnit);
            }
            deadUnits.Clear();
            
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

                entity.OnDeath += RemoveAssociatedPreviewEntityFromBattle;
                
                EventManager.Trigger(new EntityJoinBattleEvent(previewEntity,true));

                void RemoveAssociatedPreviewEntityFromBattle()
                {
                    if(!deadUnits.Contains(previewEntity)) deadUnits.Add(previewEntity);
                }
            }
            
            EventManager.Trigger(updateTurnValuesEvent);
        }

        private void RemoveEntityFromBattle(BattleEntity entity)
        {
            if(!entitiesInBattle.Contains(entity)) return;
            entitiesInBattle.Remove(entity);
            
            EventManager.Trigger(new EntityLeaveBattleEvent(entity));
        }

        private void AddDeadUnitToList(UnitDeathEvent ctx)
        {
            EventManager.Trigger(updateTurnValuesEvent);
            
            deadUnits.Add(ctx.Unit);
        }
    }

    public class PreviewEntity : BattleEntity
    {
        public Sprite Portrait => associatedEntity.Portrait;
        public int Speed => associatedEntity.Speed;
        public float DistanceFromTurnStart => associatedEntity.DistanceFromTurnStart + bm.ResetTurnValue;
        public event Action OnDeath;

        private BattleManager bm;
        private BattleEntity associatedEntity;

        public PreviewEntity(BattleManager battleManager,BattleEntity entity)
        {
            bm = battleManager;
            associatedEntity = entity;
        }

        public void InitEntityForBattle() { }

        public void KillEntityInBattle() { }

        public void ResetTurnValue(float value) { }
        public void DecayTurnValue(float amount) { }

        public IEnumerator StartRound() { yield return null;}
        public IEnumerator EndRound() { yield return null;}
        public IEnumerator StartTurn() { yield return null;}
        public IEnumerator EndTurn() { yield return null;}

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
        public event Action OnDeath;

        public void ResetTurnValue(float _)
        {
            DistanceFromTurnStart = TurnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public IEnumerator StartRound() { yield return null;}
        public IEnumerator EndRound() { yield return null;}

        public IEnumerator StartTurn() { yield return null;}

        public IEnumerator EndTurn() { yield return null;}
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

        public StartEntityTurnEvent(BattleEntity entity)
        {
            Entity = entity;
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