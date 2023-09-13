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
    
    //TODO - Remove Serialized for Debug
    [SerializeField] private UnitAbilitySO associatedAbility;

    public void LinkAbility(UnitAbilitySO ability)
    {
        associatedAbility = ability;
        
        Button.onClick.AddListener(TryCastAbility);
    }
    
    private void TryCastAbility()
    {
        EventManager.Trigger(new StartAbilitySelectionEvent(associatedAbility));
    }
}
