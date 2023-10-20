using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Battle.UIComponent
{
    using AbilityEvents;
    using ScriptableObjects.Ability;
    
    public class UIUnitAbilityButton : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        
        [Header("Description")]
        [SerializeField] private UIUnitAbilityShower abilityShower;
        
        [Header("Cooldown")]
        [SerializeField] private GameObject cooldownCoverGo;
        [SerializeField] private TextMeshProUGUI cooldownText;

        // Test a version with only one gem with cost on it
        [Header("Cost")]
        [SerializeField] private Transform abilityCostChargesParent;
        [SerializeField] private UIAbilityCostCharge[] abilityCostCharges;
        
        [Header("Ultimate")]
        [SerializeField] private GameObject ultimateChargesParentGo;
        private Transform ultimateChargesParent;
        [SerializeField] private UIUltimateCharge[] ultimateCharges;

        private UnitAbilityInstance associatedAbility;
        private Unit associatedUnit;

        private void Start()
        {
            ultimateChargesParent = ultimateChargesParentGo.transform;
            Button.onClick.AddListener(StartAbilityTargetSelection);
        }
        
        public void LinkAbility(UnitAbilityInstance ability,Unit caster)
        {
            if (associatedUnit != null)
            {
                associatedUnit.OnUltimatePointsAmountChanged -= UpdateUltimateCharges;
            }
            
            associatedAbility = ability;
            associatedUnit = caster;
            
            abilityShower.LinkAbility(ability,caster);

            associatedUnit.OnUltimatePointsAmountChanged += UpdateUltimateCharges;

            UpdateAppearance();
        }

        public void UpdateAppearance()
        {
            UpdateUltimateChargesAmount();
            UpdateCostCharges();
            UpdateCooldown();
            
            Button.interactable = associatedUnit.CurrentUltimatePoints >= associatedAbility.UltimateCost && !(associatedAbility.CurrentCooldown > 0);
        }
        
        private void UpdateUltimateChargesAmount()
        {
            var cost = associatedAbility.UltimateCost;
            for (int i = 0; i < ultimateCharges.Length; i++)
            {
                ultimateCharges[i].gameObject.SetActive(i < cost);
                ultimateCharges[i].Charge(i < associatedUnit.CurrentUltimatePoints);
            }
        }

        private void UpdateUltimateCharges(int previous,int current)
        {
            for (int i = 0; i < ultimateCharges.Length; i++)
            {
                ultimateCharges[i].Charge(i < current);
            }

            Button.interactable = associatedUnit.CurrentUltimatePoints >= associatedAbility.UltimateCost && !(associatedAbility.CurrentCooldown > 0);
        }

        private void UpdateCostCharges()
        {
            var cost = associatedAbility.Cost;
            var invert = cost < 0;
            
            for (int i = 0; i < abilityCostCharges.Length; i++)
            {
                abilityCostCharges[i].gameObject.SetActive(invert ? i < -cost : i < cost);
                abilityCostCharges[i].SetCost(cost);
            }
            
            //enable if represent with numbers (genshin tcg style)
            //if(cost == 0) abilityCostCharges[0].gameObject.SetActive(true);
        }

        private void UpdateCooldown()
        {
            cooldownCoverGo.SetActive(associatedAbility.CurrentCooldown > 0);
            cooldownText.text = $"{associatedAbility.CurrentCooldown}";
        }

        private void StartAbilityTargetSelection()
        {
            EventManager.Trigger(new StartAbilityTargetSelectionEvent(associatedAbility,associatedUnit));
        }
    }
}


