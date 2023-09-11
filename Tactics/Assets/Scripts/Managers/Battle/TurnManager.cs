using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    using BattleEvents;

    public class TurnManager : MonoBehaviour
    {
        [Header("Dependencies")] [SerializeField]
        private UnitManager unitManager;

        [SerializeField] private TileManager tileManager;

        [Header("UI Buttons")] [SerializeField]
        private Button endTurnButton;
        
        [field: Header("Debug")]
        [field: SerializeField]
        public BattleEntity CurrentEntityTurn { get; private set; }

        [field: SerializeField] public int CurrentRound { get; private set; }

        private List<BattleEntity> entitiesInBattle = new List<BattleEntity>();

        public void Start()
        {
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<StartLevelEvent>(StartBattle);
            EventManager.AddListener<EndEntityTurnEvent>(NextUnitTurn);

            endTurnButton.onClick.AddListener(EndUnitTurn);
        }

        private void StartBattle(StartLevelEvent _)
        {
            Debug.Log("Starting Level");

            entitiesInBattle.Clear();

            var roundEntity = new RoundEntity(this, 100, 1000);
            entitiesInBattle.Add(roundEntity);
            
            // TODO - add units at start of battle based on level
            entitiesInBattle.AddRange(unitManager.AllUnits);

            foreach (var unit in entitiesInBattle)
            {
                unit.ResetTurnValue(true);
                Debug.Log($"Unit {unit} turn value : {unit.TurnValue} (decay rate : {unit.DecayRate})");
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
            var activeUnits = entitiesInBattle;

            foreach (var unit in activeUnits)
            {
                unit.DecayTurnValue(decayValue);
            }
        }

        private void StartEntityTurn(BattleEntity unit)
        {
            Debug.Log($"Starting {unit}'s turn");

            CurrentEntityTurn = unit;

            CurrentEntityTurn.StartTurn();

            EventManager.Trigger(new StartEntityTurnEvent(CurrentEntityTurn));
        }
        
        private void EndUnitTurn()
        {
            Debug.Log($"Ending {CurrentEntityTurn}'s turn");
            
            CurrentEntityTurn.EndTurn();

            CurrentEntityTurn.ResetTurnValue();

            EventManager.Trigger(new EndEntityTurnEvent(CurrentEntityTurn));
        }

        private void NextUnitTurn(EndEntityTurnEvent _)
        {
            var (nextUnit, decayValue) = GetNextUnitToPlay();

            DecayTurnValues(decayValue);

            StartEntityTurn(nextUnit);
        }
    }

    public class RoundEntity : BattleEntity
    {
        public int Speed { get;}
        public float DecayRate => Speed / 100f;
        public float TurnValue { get; private set; }
        private TurnManager tm;

        public RoundEntity(TurnManager turnManager,int speed, float turnValue)
        {
            tm = turnManager;
            Speed = speed;
            TurnValue = turnValue;
        }
        
        public void ResetTurnValue(bool _ = false)
        {
            TurnValue = 1000;
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
}