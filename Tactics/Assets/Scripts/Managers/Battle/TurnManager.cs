using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    using BattleEvents;

    public class TurnManager : MonoBehaviour
    {
        [field: Header("Settings")]
        [field: SerializeField]
        public int ResetTurnValue { get; private set; } = 999;
        [field:SerializeField] public Sprite EndTurnImage { get; private set; }
        
        [Header("UI Buttons")] [SerializeField]
        private Button endTurnButton;
        
        [field: Header("Debug")]
        [field: SerializeField]
        public BattleEntity CurrentEntityTurn { get; private set; }

        private EndRoundEntity endRoundEntity;

        [field: SerializeField] public int CurrentRound { get; private set; }

        private List<BattleEntity> entitiesInBattle = new List<BattleEntity>();

        private UpdateTurnValuesEvent updateTurnValuesEvent =>
            new UpdateTurnValuesEvent(entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList(),endRoundEntity);

        public void Start()
        {
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<StartLevelEvent>(StartBattle);
            EventManager.AddListener<EndEntityTurnEvent>(NextUnitTurn);
            EventManager.AddListener<EntityJoinBattleEvent>(AddEntityToBattle);

            endTurnButton.onClick.AddListener(EndUnitTurn);
        }

        private void StartBattle(StartLevelEvent ctx)
        {
            Debug.Log("Starting Level");

            entitiesInBattle.Clear();

            endRoundEntity = new EndRoundEntity(this, 100);
            EventManager.Trigger(new EntityJoinBattleEvent(endRoundEntity,false));

            // TODO - add units at start of battle based on level

            foreach (var battleEntity in ctx.StartingEntities)
            {
                EventManager.Trigger(new EntityJoinBattleEvent(battleEntity,false));
            }
            
            CurrentRound = 0;

            EventManager.Trigger(new StartBattleEvent());

            NextRound();
        }

        private void NextRound()
        {
            CurrentRound++;

            StartRound();
        }

        private void StartRound()
        {
            Debug.Log($"Starting Round {CurrentRound}");

            EventManager.Trigger(new StartRoundEvent(CurrentRound));

            NextUnitTurn(null);
        }

        public void EndRound()
        {
            Debug.Log($"Ending Round {CurrentRound}");

            EventManager.Trigger(new EndRoundEvent(CurrentRound));

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
            CurrentEntityTurn = unit;

            CurrentEntityTurn.StartTurn();

            EventManager.Trigger(new StartEntityTurnEvent(CurrentEntityTurn));
        }
        
        private void EndUnitTurn()
        {
            CurrentEntityTurn.EndTurn();
            
            CurrentEntityTurn.ResetTurnValue(ResetTurnValue);
            
            EventManager.Trigger(new EndEntityTurnEvent(CurrentEntityTurn));
        }

        private void NextUnitTurn(EndEntityTurnEvent _)
        {
            var nextUnit = entitiesInBattle.OrderBy(entity => entity.TurnOrder).ToList().First();
            
            DecayTurnValues(nextUnit.TurnOrder);
            
            EventManager.Trigger(updateTurnValuesEvent);
            
            StartEntityTurn(nextUnit);
        }
        
        private void AddEntityToBattle(EntityJoinBattleEvent ctx)
        {
            var entity = ctx.Entity;
            
            entitiesInBattle.Add(entity);
            
            entity.ResetTurnValue(-1);
            
            EventManager.Trigger(updateTurnValuesEvent);
            
            if(ctx.Preview) return;
            var previewEntity = new PreviewEntity(this,entity);
            EventManager.Trigger(new EntityJoinBattleEvent(previewEntity,true));
        }
    }

    public class PreviewEntity : BattleEntity
    {
        public Sprite Portrait => associatedEntity.Portrait;
        public int Speed => associatedEntity.Speed;
        public float DistanceFromTurnStart => associatedEntity.DistanceFromTurnStart + tm.ResetTurnValue;
        public bool CanTakeTurn => false;

        private TurnManager tm;
        private BattleEntity associatedEntity;

        public PreviewEntity(TurnManager turnManager,BattleEntity entity)
        {
            tm = turnManager;
            associatedEntity = entity;
        }
        public void ResetTurnValue(float value) { }

        public void DecayTurnValue(float amount) { }
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
        public bool CanTakeTurn => true;
        private float turnResetValue;
        private TurnManager tm;

        public EndRoundEntity(TurnManager turnManager,int speed)
        {
            tm = turnManager;
            Portrait = tm.EndTurnImage;
            turnResetValue = tm.ResetTurnValue;
            
            Speed = speed;
            DistanceFromTurnStart = tm.ResetTurnValue;
        }
        
        public void ResetTurnValue(float _)
        {
            DistanceFromTurnStart = turnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void StartTurn()
        {
            
        }

        public void EndTurn()
        {
            tm.EndRound();
        }
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

        public StartRoundEvent(int round)
        {
            Round = round;
        }
    }

    public class EndRoundEvent
    {
        public int Round { get; }

        public EndRoundEvent(int round)
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