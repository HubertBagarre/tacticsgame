using Battle;
using Battle.AbilityEvent;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitAbilityButton : MonoBehaviour
{
    [field: SerializeField] public Button Button { get; private set; }
    [SerializeField] private Image abilityImage;
    
    [SerializeField] private GameObject[] ultimateChargesGo;
    [SerializeField] private GameObject[] ultimateChargesFillerGo;
    
    private UnitAbilityInstance associatedAbility;
    private Unit associatedUnit;

    public void LinkAbility(UnitAbilityInstance ability,Unit caster)
    {
        associatedAbility = ability;
        associatedUnit = caster;
        
        Button.onClick.AddListener(TryCastAbility);
    }
    
    private void TryCastAbility()
    {
        EventManager.Trigger(new StartAbilitySelectionEvent(associatedAbility,associatedUnit));
    }
}
