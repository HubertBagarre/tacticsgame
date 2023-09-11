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

        private BattleEntity roundEntity;

        [field: SerializeField] public int CurrentRound { get; private set; }

        private List<BattleEntity> entitiesInBattle = new List<BattleEntity>();

        private UpdateTurnValuesEvent updateTurnValuesEvent =>
            new UpdateTurnValuesEvent(entitiesInBattle.OrderBy(entity => entity.TurnValue).ToList(),roundEntity);

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

            roundEntity = new RoundEntity(this, 100);
            EventManager.Trigger(new EntityJoinBattleEvent(roundEntity));
            
            // TODO - add units at start of battle based on level

            foreach (var battleEntity in ctx.StartingEntities)
            {
                EventManager.Trigger(new EntityJoinBattleEvent(battleEntity));
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

        public (BattleEntity entity, float decayTime) GetNextUnitToPlay()
        {
            var activeUnits = entitiesInBattle;
            var fastestUnit = activeUnits.First();
            var timeForDecay = fastestUnit.TurnValue / fastestUnit.DecayRate;
            var smallestTimeForDecay = timeForDecay;

            foreach (var unit in activeUnits)
            {
                timeForDecay = unit.TurnValue / unit.DecayRate;

                if (!(timeForDecay < smallestTimeForDecay)) continue;
                fastestUnit = unit;
                smallestTimeForDecay = timeForDecay;
            }

            return (fastestUnit, smallestTimeForDecay);
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
            var (nextUnit, decayValue) = GetNextUnitToPlay();

            DecayTurnValues(decayValue);

            StartEntityTurn(nextUnit);
        }

        private void AddEntityToBattle(EntityJoinBattleEvent ctx)
        {
            var entity = ctx.Entity;
            
            entitiesInBattle.Add(entity);
            
            entity.ResetTurnValue(-1);
            
            EventManager.Trigger(updateTurnValuesEvent);
        }
    }

    public class RoundEntity : BattleEntity
    {
        public Sprite Portrait { get; }
        public int Speed { get;}
        public float DecayRate => Speed / 100f;
        public float TurnValue { get; private set; }
        private float turnResetValue;
        private TurnManager tm;

        public RoundEntity(TurnManager turnManager,int speed)
        {
            tm = turnManager;
            Portrait = tm.EndTurnImage;
            turnResetValue = tm.ResetTurnValue;
            
            Speed = speed;
            TurnValue = tm.ResetTurnValue;
        }
        
        public void ResetTurnValue(float _)
        {
            TurnValue = turnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            TurnValue -= amount * DecayRate;
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
        
        public EntityJoinBattleEvent(BattleEntity entity)
        {
            Entity = entity;
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