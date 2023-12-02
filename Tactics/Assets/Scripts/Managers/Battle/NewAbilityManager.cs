using System;
using Battle;
using Battle.InputEvent;
using UnityEngine;

public class NewAbilityManager : MonoBehaviour
{
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
        
        Debug.Log("Starting");
        
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
        
        currentAbilityInstance = null;
        currentCaster = null;
        
        if(ability == null || caster == null) return;
        
        Debug.Log($"Ending {(canceled ? "(canceled)" : "")} ");
        
        ability.EndTileSelection();
        
        EventManager.Trigger(new EndAbilityTargetSelectionEvent(ability,caster,canceled));
    }
    
    public static void RequestStartAbilityTargetSelection(AbilityInstance abilityInstance, NewUnit caster) => OnStartAbilityTargetSelection?.Invoke(abilityInstance,caster);
    public static void RequestEndAbilityTargetSelection(bool canceled) => OnEndAbilityTargetSelection?.Invoke(canceled);
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