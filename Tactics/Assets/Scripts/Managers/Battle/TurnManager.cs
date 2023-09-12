using System.Collections;
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

            endTurnButton.onClick.AddListener(EndCurrentEntityTurn);
        }

        private void StartBattle(StartLevelEvent ctx)
        {
            Debug.Log("Starting Level");

            entitiesInBattle.Clear();

            endRoundEntity = new EndRoundEntity(this, 100);
            AddEntityToBattle(endRoundEntity,false);

            // TODO - add units at start of battle based on level

            foreach (var battleEntity in ctx.StartingEntities)
            {
                AddEntityToBattle(battleEntity,true);
            }
            
            CurrentRound = 0;

            EventManager.Trigger(new StartBattleEvent());

            EventManager.AddListener<RoundStartEvent>(StartEntityTurnAtRoundStart,true);
            
            NextRound();
            
            void StartEntityTurnAtRoundStart(RoundStartEvent ctx)
            {
                Debug.Log("Called at 1st round start event");
                
                NextUnitTurn();
            }
        }
        
        private void NextRound()
        {
            CurrentRound++;

            StartCoroutine(NextRoundAnimationsRoutine());

            IEnumerator NextRoundAnimationsRoutine()
            {
                yield return new WaitForSeconds(1f);
                
                StartRound();
            }
        }

        private void StartRound()
        {
            EventManager.Trigger(new RoundStartEvent(CurrentRound));
        }
        
        public void EndRound()
        {
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
            CurrentEntityTurn = unit;
            
            EventManager.Trigger(new StartEntityTurnEvent(CurrentEntityTurn));
            
            CurrentEntityTurn.StartTurn();
        }
        
        public void EndCurrentEntityTurn()
        {
            CurrentEntityTurn.ResetTurnValue(ResetTurnValue);
            
            EventManager.Trigger(new EndEntityTurnEvent(CurrentEntityTurn));
            
            CurrentEntityTurn.EndTurn();
            
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
            entitiesInBattle.Add(entity);
            entity.ResetTurnValue(-1);

            EventManager.Trigger(new EntityJoinBattleEvent(entity,false));
            
            if (createPreview)
            {
                var previewEntity = new PreviewEntity(this,entity);
                entitiesInBattle.Add(previewEntity);
                
                EventManager.Trigger(new EntityJoinBattleEvent(previewEntity,true));
            }
            
            EventManager.Trigger(updateTurnValuesEvent);
        }
    }

    public class PreviewEntity : BattleEntity
    {
        public Sprite Portrait => associatedEntity.Portrait;
        public int Speed => associatedEntity.Speed;
        public float DistanceFromTurnStart => associatedEntity.DistanceFromTurnStart + tm.ResetTurnValue;

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
        private float TurnResetValue => tm.ResetTurnValue;
        private TurnManager tm;

        public EndRoundEntity(TurnManager turnManager,int speed)
        {
            tm = turnManager;
            Portrait = tm.EndTurnImage;

            Speed = speed;
            DistanceFromTurnStart = TurnResetValue;
        }
        
        public void ResetTurnValue(float _)
        {
            DistanceFromTurnStart = TurnResetValue;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void StartTurn()
        {
            EventManager.AddListener<RoundStartEvent>(EndThisUnitTurn,true);
            
            tm.EndRound(); // End Current Round and Start next round
            
            //End turn

            void EndThisUnitTurn(RoundStartEvent ctx)
            {
                tm.EndCurrentEntityTurn();
            }
        }

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

    public class RoundStartEvent
    {
        public int Round { get; }

        public RoundStartEvent(int round)
        {
            Round = round;
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