using System.Collections.Generic;
using Battle;
using Battle.UIComponent;
using UnityEngine;

public class NewUIBattleManager : MonoBehaviour
{
    [Header("Unit UI")]
    [SerializeField] private UIUnit uiUnitPrefab;
    private Dictionary<NewUnit, UIUnit> uiUnitsDict = new ();

    public void AddCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.OnInvoked += InstantiateUnitUi;
    }
    
    public void RemoveCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.OnInvoked -= InstantiateUnitUi;
    }
    
    private void InstantiateUnitUi(NewBattleManager.UnitCreatedAction action)
    {
        var unit = action.Unit;
        if(uiUnitsDict.ContainsKey(unit)) uiUnitsDict[unit].gameObject.SetActive(false);
        var ui = Instantiate(uiUnitPrefab, action.Renderer.UiParent);
        ui.LinkToUnit(unit);
            
        uiUnitsDict.Add(unit,ui);
    }
}
