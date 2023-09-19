using System;
using Battle;
using Battle.AbilityEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnitAbilityButton : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [SerializeField] private Image abilityImage;
        
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
            if (associatedAbility != null)
            {
                //remove callbacks
            }
            
            associatedAbility = ability;
            associatedUnit = caster;

            UpdateUltimateCharges();
            UpdateCostCharges();
            UpdateCooldown();

            //add callbacks
        }

        private void UpdateUltimateCharges()
        {
            var cost = associatedAbility.UltimateCost;
            for (int i = 0; i < ultimateCharges.Length; i++)
            {
                ultimateCharges[i].gameObject.SetActive(i < cost);
            }
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
            if(cost == 0) abilityCostCharges[0].gameObject.SetActive(true);
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


