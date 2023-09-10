using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private UnitManager unitManager;

    [Header("UI Buttons")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button pauseButton;

    [Header("Debug")]
    [SerializeField] private bool running = false;
    [SerializeField] private bool unitTurn = false;
    [SerializeField] private int currentTurn;
    
    public void Start()
    {   
       AddCallbacks();
    }

    private void AddCallbacks()
    {
        EventManager.AddListener<StartBattleEvent>(StartBattle);
        EventManager.AddListener<EndUnitTurnEvent>(ResumeUpdate);
        
        pauseButton.onClick.AddListener(TogglePause);
        endTurnButton.onClick.AddListener(EndUnitTurn);
    }

    private void StartBattle(StartBattleEvent _)
    {
        Debug.Log("Starting Battle");
        
        currentTurn = 0;

        foreach (var unit in unitManager.AllUnits)
        {
            unit.ResetTurnValue(true);
        }

        running = true;
    }

    private void Update()
    {
        if(!running) return;
        if(unitTurn) return;
        
        DecayTurnValues();
    }

    private void TogglePause()
    {
        running = !running;
    }

    private void DecayTurnValues()
    {
        var activeUnits = unitManager.AllUnits.Where(unit => unit.IsActive).ToList();
        var fastestUnit = activeUnits.First();
        var timeForDecay = fastestUnit.TurnValue/fastestUnit.DecayRate;
        var smallestTimeForDecay = timeForDecay;
        
        
        foreach (var unit in activeUnits)
        {
            timeForDecay = unit.TurnValue / unit.DecayRate;
            
            if (!(timeForDecay < smallestTimeForDecay)) continue;
            fastestUnit = unit;
            smallestTimeForDecay = timeForDecay;
        }
        
        foreach (var unit in activeUnits)
        {
            unit.DecayTurnValue(smallestTimeForDecay);
        }
        
        fastestUnit.ResetTurnValue();
        
        StartUnitTurn(fastestUnit);
    }

    private void StartUnitTurn(Unit unit)
    {
        unitTurn = true;
        
        Debug.Log($"{unit}'s Turn");
        
        EventManager.Trigger(new StartUnitTurnEvent(unit));
    }

    private void EndUnitTurn()
    {
        EventManager.Trigger(new EndUnitTurnEvent());
    }

    private void ResumeUpdate(EndUnitTurnEvent _)
    {
        unitTurn = false;
    }
}

public class StartUnitTurnEvent
{
    public Unit Unit { get; private set; }

    public StartUnitTurnEvent(Unit unit)
    {
        Unit = unit;
    }
}

public class EndUnitTurnEvent { }
