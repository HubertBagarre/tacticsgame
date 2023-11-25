using System.Collections.Generic;
using Battle;
using Battle.InputEvent;
using Battle.UIComponent;
using UnityEngine;

public class NewUIBattleManager : MonoBehaviour
{
    [Header("Unit UI")]
    [SerializeField] private UIUnit uiUnitPrefab;
    private Dictionary<NewUnit, UIUnit> uiUnitsDict = new ();
    
    [Header("Unit Tooltip")]
    [SerializeField] private UIUnitTooltip unitTooltip;
    
    [Header("Tooltip")]
    [SerializeField] private UITooltip tooltip;
    public static UITooltip Tooltip { get; private set; }

    public void AddCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.OnInvoked += InstantiateUnitUi;
        
        EventManager.AddListener<ClickUnitEvent>(ShowUnitTooltip);
    }
    
    public void RemoveCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.OnInvoked -= InstantiateUnitUi;
        
        EventManager.RemoveListener<ClickUnitEvent>(ShowUnitTooltip);
    }
    
    public void Start()
    {
        AssignTooltip();
        
        unitTooltip.Hide();
    }
    
    private void InstantiateUnitUi(NewBattleManager.UnitCreatedAction action)
    {
        var unit = action.Unit;
        if(uiUnitsDict.ContainsKey(unit)) uiUnitsDict[unit].gameObject.SetActive(false);
        var ui = Instantiate(uiUnitPrefab, action.Renderer.UiParent);
        ui.LinkToUnit(unit);
            
        uiUnitsDict.Add(unit,ui);
    }
    
    private void ShowUnitTooltip(ClickUnitEvent ctx)
    {
        var unit = ctx.Unit;
        
        if (unit == null)
        {
            unitTooltip.Hide();
            return;
        }
            
        unitTooltip.DisplayUnitTooltip(ctx.Unit);
        unitTooltip.Show();
    }
    
    private void AssignTooltip()
    {
        Tooltip = tooltip;
        Tooltip.Hide();
    }
}
