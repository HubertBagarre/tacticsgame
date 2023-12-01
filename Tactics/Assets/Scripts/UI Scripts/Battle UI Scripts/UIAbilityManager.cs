using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.UIComponent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityManager : MonoBehaviour
{
    [Header("Ability Tile Selection")]
    [SerializeField] private GameObject abilityTargetSelectionUIObj;
    [SerializeField] private Button confirmTileSelectionButton;
    [SerializeField] private Button cancelTileSelectionButton;
    [SerializeField] private TextMeshProUGUI selectionsLeftText;
    
    [Header("Ability Buttons")]
    [SerializeField] private UIUnitAbilityButton abilityButtonPrefab;
    [SerializeField] private Transform abilityButtonParent;
    
    [Header("Ability Points")]
    [SerializeField] private UIAbilityPointCharge abilityPointChargePrefab;
    [SerializeField] private Transform abilityPointChargeParent;
    
    private Dictionary<AbilityInstance, UIUnitAbilityButton> unitAbilityButtonsDict = new();
    private UIAbilityPointCharge[] abilityPointCharges;
    
    private void Start()
    {
        
    }
    
    public void AddCallbacks()
    {
        
    }

    public void RemoveCallbacks()
    {
        
    }
    
    public void ShowAbilityTargetSelection(bool value = true)
    {
        abilityTargetSelectionUIObj.SetActive(value);
    }
    
    public void ShowUnitAbilitiesButton(NewUnit unit)
    {
        abilityTargetSelectionUIObj.SetActive(true);

        foreach (var abilityInstance in unit.AbilityInstances.Where(instance => instance.ShowInTooltip))
        {
            ShowAbilityButton(abilityInstance,unit);
        }
    }
    public void HideUnitAbilitiesButton()
    {
        foreach (var abilityButton in unitAbilityButtonsDict.Values)
        {
            abilityButton.gameObject.SetActive(false);
        }
    }

    public void ShowAbilityButton(AbilityInstance abilityInstance,NewUnit owner)
    {
        var hasButton = unitAbilityButtonsDict.TryGetValue(abilityInstance,out var abilityButton);
        
        if (!hasButton)
        {
            abilityButton = Instantiate(abilityButtonPrefab, abilityButtonParent);
            abilityButton.LinkAbility(abilityInstance,owner);
            unitAbilityButtonsDict.Add(abilityInstance,abilityButton);
        }
        
        abilityButton.gameObject.SetActive(true);
        abilityButton.UpdateAppearance();
    }
}
