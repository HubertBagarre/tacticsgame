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

    public void LinkAbility(UnitAbilityInstance ability,Unit caster)
    {
        associatedAbility = ability;
        associatedUnit = caster;
        
        Button.onClick.AddListener(StartAbilityTargetSelection);
    }
    
    private void StartAbilityTargetSelection()
    {
        EventManager.Trigger(new StartAbilitySelectionEvent(associatedAbility,associatedUnit));
    }
}
