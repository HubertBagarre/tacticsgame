using System;
using Battle;
using Battle.AbilityEvent;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitAbilityButton : MonoBehaviour
{
    [field: SerializeField] public Button Button { get; private set; }
    [SerializeField] private Image abilityImage;
    // TODO - Cooldown text here
    
    [SerializeField] private GameObject[] ultimateChargesGo;
    [SerializeField] private GameObject[] ultimateChargesFillerGo;
    
    private UnitAbilityInstance associatedAbility;
    private Unit associatedUnit;

    private void Start()
    {
        Button.onClick.AddListener(StartAbilityTargetSelection);
    }

    public void LinkAbility(UnitAbilityInstance ability,Unit caster)
    {
        associatedAbility = ability;
        associatedUnit = caster;
    }
    
    private void StartAbilityTargetSelection()
    {
        Debug.Log("CLICK");
        EventManager.Trigger(new StartAbilityTargetSelectionEvent(associatedAbility,associatedUnit));
        Debug.Log("After CLICK");
    }
}
