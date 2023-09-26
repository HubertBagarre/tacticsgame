using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    using AbilityEvents;
    
    public class UIUnitAbilityButton : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [SerializeField] private Image abilityImage;

        [Header("Description")]
        [SerializeField] private GameObject descriptionPanelGo;
        [SerializeField] private RectTransform descriptionPanelTr;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        
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

        private bool isHoveringDescription = false;

        private void Start()
        {
            ultimateChargesParent = ultimateChargesParentGo.transform;
            Button.onClick.AddListener(StartAbilityTargetSelection);
            
            descriptionPanelGo.SetActive(false);
        }

        public void ShowDescription(BaseEventData _)
        {
            UIBattleManager.Tooltip.Hide();
            
            descriptionPanelGo.SetActive(true);
        }
        
        public void HideDescription(BaseEventData _)
        {
            StartCoroutine(WaitFrame());
            
            IEnumerator WaitFrame()
            {
                yield return null;
                if(isHoveringDescription) yield break;

                HideDescription();
            }
        }

        private void HideDescription()
        {
            UIBattleManager.Tooltip.Hide();
            descriptionPanelGo.SetActive(isHoveringDescription);
        }

        public void HoverDescription(BaseEventData _)
        {
            isHoveringDescription = true;
        }

        public void ExitHoverDescription(BaseEventData _)
        {
            isHoveringDescription = false;
            HideDescription();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (UIBattleManager.Tooltip.IsEnabled)
            {
                UIBattleManager.Tooltip.Hide();
                return;
            }
            
            var mousePos = (Vector3) eventData.position;

            var linkedTarget = TMP_TextUtilities.FindIntersectingLink(abilityDescriptionText, mousePos,null);
            
            if(linkedTarget == -1) return;
            
            var info = abilityDescriptionText.textInfo.linkInfo[linkedTarget];
            
            UIBattleManager.Tooltip.Show(mousePos,associatedAbility.SO.ConvertDescriptionLinks(associatedUnit,info.GetLinkID()));
        }
        
        public void LinkAbility(UnitAbilityInstance ability,Unit caster)
        {
            if (associatedUnit != null)
            {
                associatedUnit.OnUltimatePointsAmountChanged -= UpdateUltimateCharges;
            }
            
            if (associatedAbility != null)
            {
                //remove callbacks
            }
            
            associatedAbility = ability;
            associatedUnit = caster;

            associatedUnit.OnUltimatePointsAmountChanged += UpdateUltimateCharges;

            UpdateAppearance();
        }

        public void UpdateAppearance()
        {
            abilityImage.sprite = associatedAbility.SO.Sprite;
            
            UpdateUltimateChargesAmount();
            UpdateCostCharges();
            UpdateCooldown();

            UpdateDescription();
            
            Button.interactable = associatedUnit.CurrentUltimatePoints >= associatedAbility.UltimateCost && !(associatedAbility.CurrentCooldown > 0);
        }

        private void UpdateDescription()
        {
            var so = associatedAbility.SO;

            var color = Color.yellow;
            var col = ColorUtility.ToHtmlStringRGB(color);
            abilityNameText.text = $"{so.Name} <size=20><color=#{col}><i>[{so.Type}]</i></color></size>";

            var cooldown = so.Cooldown;
            abilityCooldownText.text = cooldown > 0 ? $"{so.Cooldown} turn{(cooldown > 1 ? "s":"")}" : "";
            
            StartCoroutine(AdjustPanelSizeRoutine());

            IEnumerator AdjustPanelSizeRoutine()
            {
                var description = so.ConvertedDescription(associatedUnit);
                abilityDescriptionText.text = $"{description}";
                
                abilityDescriptionText.ForceMeshUpdate();

                yield return null;

                var lineCount = abilityDescriptionText.textInfo.lineCount;
                
                Debug.Log($"{associatedAbility.SO.Name} has {lineCount} lines");
                
                var size = descriptionPanelTr.sizeDelta;
                
                if (lineCount <= 2)
                {
                    size.y = 100;
                    descriptionPanelTr.sizeDelta = size;
                    yield break;
                }

                lineCount -= 2;
                var lineSize = abilityDescriptionText.fontSize;
                var sizeIncrease = lineCount * lineSize + 2;

                size.y = 100 + sizeIncrease;
                descriptionPanelTr.sizeDelta = size;
            }
            
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


