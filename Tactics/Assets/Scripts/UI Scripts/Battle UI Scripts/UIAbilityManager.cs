using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.ScriptableObjects;
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
        AddCallbacks();
    }
    
    public void AddCallbacks()
    {
        cancelTileSelectionButton.onClick.AddListener(CancelAbilityTargetSelection);
        confirmTileSelectionButton.onClick.AddListener(ConfirmAbilityTargetSelection);
        
        EventManager.AddListener<StartAbilityTargetSelectionEvent>(AddAbilityCallbacks);
        EventManager.AddListener<EndAbilityTargetSelectionEvent>(RemoveAbilityCallbacks);
    }

    public void RemoveCallbacks()
    {
        cancelTileSelectionButton.onClick.RemoveListener(CancelAbilityTargetSelection);
        confirmTileSelectionButton.onClick.RemoveListener(ConfirmAbilityTargetSelection);
        
        EventManager.RemoveListener<StartAbilityTargetSelectionEvent>(AddAbilityCallbacks);
        EventManager.RemoveListener<EndAbilityTargetSelectionEvent>(RemoveAbilityCallbacks);
    }
    
    private void CancelAbilityTargetSelection()
    {
        NewAbilityManager.RequestEndAbilityTargetSelection(true);
    }
    
    private void ConfirmAbilityTargetSelection()
    {
        NewAbilityManager.RequestEndAbilityTargetSelection(false);
    }
    
    public void ShowAbilityTargetSelection(bool value = true)
    {
        abilityTargetSelectionUIObj.SetActive(value);
    }
    
    public void ShowUnitAbilitiesButton(NewUnit unit)
    {
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
    
    private void AddAbilityCallbacks(StartAbilityTargetSelectionEvent startAbilityTargetSelectionEvent)
    {
        var abilityInstance = startAbilityTargetSelectionEvent.AbilityInstance;
        
        abilityInstance.OnCurrentSelectedTilesUpdated += UpdateAbilityTargetSelection;
    }
    
    private void RemoveAbilityCallbacks(EndAbilityTargetSelectionEvent endAbilityTargetSelectionEvent)
    {
        var abilityInstance = endAbilityTargetSelectionEvent.AbilityInstance;
        
        abilityInstance.OnCurrentSelectedTilesUpdated -= UpdateAbilityTargetSelection;
    }
    
    private void UpdateAbilityTargetSelection(AbilityInstance abilityInstance)
    {
        var ability = abilityInstance;
        
        var selectionsLeft = ability.ExpectedSelections - ability.CurrentSelectionCount;
        if(selectionsLeft < 0) selectionsLeft = 0;
        
        var text = "Select %COUNT% %TARGET%.";
        text = ability.SO.SelectionCondition.OverrideTargetText(text,selectionsLeft);
        
        selectionsLeftText.text = selectionsLeft != 0 ? text : string.Empty;
        
        confirmTileSelectionButton.interactable = selectionsLeft == 0;
    }
}
