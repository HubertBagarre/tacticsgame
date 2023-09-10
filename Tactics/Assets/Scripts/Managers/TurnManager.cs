using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private UnitManager unitManager;


    [Header("Debug")]
    [SerializeField] private bool running = false;
    [SerializeField] private bool unitTurn = false;
    [SerializeField] private int currentTurn;

    private Dictionary<Unit, float> unitTurnValueDict = new Dictionary<Unit, float>();

    public void Start()
    {   
        EventManager.AddListener<StartLevelEvent>(StartBattle);
    }

    private void StartBattle(StartLevelEvent _)
    {
        Debug.Log("Starting Battle");
        
        unitTurnValueDict.Clear();
        currentTurn = 0;

        foreach (var unit in unitManager.AllUnits)
        {
            unitTurnValueDict.Add(unit,unit.Stats.BaseTurnValue);
        }
    }

    private void Update()
    {
        if(!running) return;
        
        DecayTurnValues();
        
        
    }

    private void DecayTurnValues()
    {
        foreach (var unit in unitTurnValueDict.Keys.Where(unit => unit.Active))
        {
            unitTurnValueDict[unit] -= 1 * unit.DecayRate;

            if (!(unitTurnValueDict[unit] <= 0)) continue;
            
            StartUnitTurn(unit);
            
            unitTurnValueDict[unit] = unit.Stats.BaseTurnValue;
            
            return;
        }
    }

    private void StartUnitTurn(Unit unit)
    {
        unitTurn = true;
    }
}
