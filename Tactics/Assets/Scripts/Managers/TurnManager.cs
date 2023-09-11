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

        [SerializeField] private Button pauseButton;

        [field: Header("Debug")]
        [field: SerializeField]
        public Unit CurrentUnitTurn { get; private set; }

        [field: SerializeField] public int CurrentRound { get; private set; }

        public void Start()
        {
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<StartLevelEvent>(StartBattle);
            EventManager.AddListener<EndUnitTurnEvent>(NextUnitTurn);

            endTurnButton.onClick.AddListener(EndUnitTurn);
        }

        private void StartBattle(StartLevelEvent _)
        {
            Debug.Log("Starting Level");

            foreach (var unit in unitManager.AllUnits)
            {
                unit.ResetTurnValue(true);
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

        public (Unit unit, float decayTime) GetNextUnitToPlay()
        {
            var activeUnits = unitManager.AllUnits.Where(unit => unit.IsActive).ToList();
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
            var activeUnits = unitManager.AllUnits.Where(unit => unit.IsActive).ToList();

            foreach (var unit in activeUnits)
            {
                unit.DecayTurnValue(decayValue);
            }
        }

        private void StartUnitTurn(Unit unit)
        {
            Debug.Log($"Starting {unit}'s turn");

            CurrentUnitTurn = unit;

            CurrentUnitTurn.StartTurn();

            EventManager.Trigger(new StartUnitTurnEvent(CurrentUnitTurn));
        }

        private void EndUnitTurn()
        {
            Debug.Log($"Ending {CurrentUnitTurn}'s turn");

            CurrentUnitTurn.ResetTurnValue();

            EventManager.Trigger(new EndUnitTurnEvent(CurrentUnitTurn));
        }

        private void NextUnitTurn(EndUnitTurnEvent _)
        {
            var (nextUnit, decayValue) = GetNextUnitToPlay();

            DecayTurnValues(decayValue);

            StartUnitTurn(nextUnit);
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

    public class StartUnitTurnEvent
    {
        public Unit Unit { get; }

        public StartUnitTurnEvent(Unit unit)
        {
            Unit = unit;
        }
    }

    public class EndUnitTurnEvent
    {
        public Unit Unit { get; }

        public EndUnitTurnEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}