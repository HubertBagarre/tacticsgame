using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.InputEvent;
using Battle.ScriptableObjects;
using MoonSharp.Interpreter;
using UnityEngine;

public class NewAbilityManager : MonoBehaviour
{
    [SerializeField] private TextAsset luaScript;
    
    [SerializeField] private int currentAbilityPoints = 0;
    public int CurrentAbilityPoints
    {
        get => currentAbilityPoints;
        private set
        {
            var previous = currentAbilityPoints;
            currentAbilityPoints = value;
            OnUpdatedAbilityPoints?.Invoke(previous, currentAbilityPoints);
        }
    }

    [field: SerializeField] public int MaxAbilityPoints { get; private set; } = 6;
    public static event Action<int, int> OnUpdatedAbilityPoints;
    
    private static event Action<AbilityInstance,NewUnit> OnStartAbilityTargetSelection;
    private static event Action<bool> OnEndAbilityTargetSelection;
    private NewUnit currentCaster;
    private AbilityInstance currentAbilityInstance;
    public void Setup(int startingAbilityPoints)
    {
        currentAbilityPoints = startingAbilityPoints;
        
        UserData.RegisterAssembly();
    }

    public void AddCallbacks()
    {
        OnStartAbilityTargetSelection += StartAbilityTargetSelection;
        OnEndAbilityTargetSelection += EndAbilityTargetSelection;
    }

    public void RemoveCallbacks()
    {
        OnStartAbilityTargetSelection -= StartAbilityTargetSelection;
        OnEndAbilityTargetSelection -= EndAbilityTargetSelection;
    }

    private void StartAbilityTargetSelection(AbilityInstance abilityInstance, NewUnit caster)
    {
        currentAbilityInstance = abilityInstance;
        currentCaster = caster;
        
        EventManager.Trigger(new StartAbilityTargetSelectionEvent(currentAbilityInstance, currentCaster));
        
        currentAbilityInstance.ClearSelection();
        
        if (currentCaster.CurrentUltimatePoints < currentAbilityInstance.UltimateCost)
        {
            EndAbilityTargetSelection(true);
            return;
        }
        
        //EventManager.AddListener<EndAbilityTargetSelectionEvent>(TryCastAbility, true);

        //TODO add event to cancel ability on caster death

        currentAbilityInstance.StartTileSelection(caster);
        
        EventManager.AddListener<ClickTileEvent>(SelectTile);
    }
    
    private void SelectTile(ClickTileEvent clickEvent)
    {
        var tile = clickEvent.Tile;
        
        if (tile == null) return;
        
        currentAbilityInstance.TryAddTileToSelection(tile);
    }
    
    private void EndAbilityTargetSelection(bool canceled)
    {
        var ability = currentAbilityInstance;
        var caster = currentCaster;
        var selectedTiles = ability.CurrentSelectedTiles.ToList();
        
        currentAbilityInstance = null;
        currentCaster = null;
        
        if(ability == null || caster == null) return;
        
        Debug.Log($"Ending Ability Selection {(canceled ? "(canceled)" : "")} ");
        
        ability.EndTileSelection();
        
        if(!canceled)
        {
            var castAbilityAction = new CastAbilityAction(ability,caster,selectedTiles,luaScript.text);
            castAbilityAction.TryStack();
        }
        
        EventManager.Trigger(new EndAbilityTargetSelectionEvent(ability,caster,canceled));
    }
    
    public static void RequestStartAbilityTargetSelection(AbilityInstance abilityInstance, NewUnit caster) => OnStartAbilityTargetSelection?.Invoke(abilityInstance,caster);
    public static void RequestEndAbilityTargetSelection(bool canceled) => OnEndAbilityTargetSelection?.Invoke(canceled);
}

public class CastAbilityAction : SimpleStackableAction
{
    protected override YieldInstruction YieldInstruction { get; } /*=> new WaitForSeconds(1f);*/
    protected override CustomYieldInstruction CustomYieldInstruction { get; }
    
    public AbilityInstance AbilityInstance { get; }
    public NewUnit Caster { get; }
    public List<NewTile> TargetTiles { get; }
    protected string LuaScript { get; }
    
    public CastAbilityAction(AbilityInstance abilityInstance,NewUnit caster,List<NewTile> targetTiles,string luaScript)
    {
        AbilityInstance = abilityInstance;
        Caster = caster;
        TargetTiles = targetTiles;
        LuaScript = luaScript;
    }
    
    protected override void Main()
    {
        Debug.Log($"Casting Main");
        
        // add so effects
        // add additionnal effects
        
        //add yielded actions
    }

    protected override void PostWaitAction()
    {
        var effects = AbilityInstance.GetEffects(Caster, TargetTiles.ToArray(),LuaScript);
        
        Debug.Log($"{Caster} is casting {AbilityInstance.SO.Name}({effects.Count}) on {TargetTiles.Count}");
        
        foreach (var effect in effects)
        {
            effect.ApplyEffects(Caster, TargetTiles.ToArray());
        }
    }
}

public class StartAbilityTargetSelectionEvent
{
    public AbilityInstance AbilityInstance { get; }
    public NewUnit Caster { get; }

    public StartAbilityTargetSelectionEvent(AbilityInstance abilityInstance, NewUnit caster)
    {
        AbilityInstance = abilityInstance;
        Caster = caster;
    }
}

public class EndAbilityTargetSelectionEvent
{
    public AbilityInstance AbilityInstance { get; }
    public NewUnit Caster { get; }
    public bool Canceled { get; }

    public EndAbilityTargetSelectionEvent(AbilityInstance abilityInstance, NewUnit caster,bool canceled)
    {
        AbilityInstance = abilityInstance;
        Caster = caster;
        Canceled = canceled;
    }
}