using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Battle.UIComponent
{
    using AbilityEvents;
    using UIEvents;
    
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

        private AbilityInstance associatedAbility;
        private NewUnit associatedUnit;

        private void Start()
        {
            ultimateChargesParent = ultimateChargesParentGo.transform;
            Button.onClick.AddListener(StartAbilityTargetSelection);
        }

        private bool CanBeCast()
        {
            //TODO: rework this abilities to work with NewUnit
            if (!associatedAbility.SO.MatchesRequirements(associatedUnit)) return false;
            
            return true;

            /*if(!associatedAbility.SO.CanCastAbility(associatedUnit)) return false;
            if (associatedAbility.CurrentCooldown > 0) return false;
            return associatedUnit.CurrentUltimatePoints >= associatedAbility.UltimateCost;*/
        }
        
        public void LinkAbility(AbilityInstance ability,NewUnit owner)
        {
            //TODO: rework this abilities to work with NewUnit
            
            if (associatedUnit != null)
            {
                associatedUnit.OnUltimatePointsAmountChanged -= UpdateUltimateCharges;
                //EventManager.RemoveListener<EndAbilityTargetSelectionEvent>(UpdateButtonInteractable);
            }
            
            associatedAbility = ability;
            associatedUnit = owner;
            
            abilityShower.LinkAbility(ability,owner);

            associatedUnit.OnUltimatePointsAmountChanged += UpdateUltimateCharges;
            //EventManager.AddListener<EndAbilityTargetSelectionEvent>(UpdateButtonInteractable);

            UpdateAppearance();
        }

        public void UpdateAppearance()
        {
            UpdateUltimateChargesAmount();
            UpdateCostCharges();
            UpdateCooldown();
            
            Button.interactable = CanBeCast();
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
            //TODO: rework this abilities to work with NewUnit
            
            Debug.Log($"Requesting selection for {associatedAbility.SO.Name}");
            
            //EventManager.Trigger(new StartAbilityTargetSelectionEvent(associatedAbility,associatedUnit));
        }
    }
}


