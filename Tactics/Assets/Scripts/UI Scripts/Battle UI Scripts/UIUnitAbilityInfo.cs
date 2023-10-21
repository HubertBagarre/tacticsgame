using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UIComponent
{
    using ScriptableObjects;
    
    public class UIUnitAbilityInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
    {
        [SerializeField] private UIUnitAbilityShower abilityShower;
        
        [Header("Description")]
        [SerializeField] private GameObject descriptionPanelGo;
        [SerializeField] private RectTransform descriptionPanelTr;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        
        private UnitAbilityInstance associatedAbility => abilityShower.AssociatedAbility;
        private Unit associatedUnit => abilityShower.AssociatedUnit;

        public bool IsHovering { get; private set; }
        [SerializeField] private bool show = true;

        private void OnDisable()
        {
            HideDescription();
            IsHovering = false;
        }

        public void ShowDescription()
        {
            UIBattleManager.Tooltip.Hide();
            
            descriptionPanelGo.SetActive(true);
            UpdateDescription();
        }
        
        public void HideDescription()
        {
            UIBattleManager.Tooltip.Hide();
            descriptionPanelGo.SetActive(false);
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
        
        private void UpdateDescription()
        {
            var so = associatedAbility.SO;

            abilityNameText.text = $"{so.Name}";
            if (so.Type != AbilityType.None)
            {
                var color = so.Type switch
                {
                    AbilityType.Movement => Color.yellow,
                    _ => Color.yellow
                };
                var col = ColorUtility.ToHtmlStringRGB(color);
                abilityNameText.text = $"{so.Name} <size=20><color=#{col}><i>[{so.Type}]</i></color></size>";
            }
            
            var cooldown = so.Cooldown;
            abilityCooldownText.text = cooldown > 0 ? $"{so.Cooldown} turn{(cooldown > 1 ? "s":"")}" : "";
            
            StartCoroutine(AdjustPanelSizeRoutine());
            return;
            
            IEnumerator AdjustPanelSizeRoutine()
            {
                var description = so.ConvertedDescription(associatedUnit);
                abilityDescriptionText.text = $"{description}";
                
                abilityDescriptionText.ForceMeshUpdate();

                yield return null;

                var textInfo = abilityDescriptionText.textInfo;
                var lineCount = textInfo.lineCount;
                
                var size = descriptionPanelTr.sizeDelta;
                
                if (lineCount <= 2)
                {
                    size.y = 100;
                    descriptionPanelTr.sizeDelta = size;
                    yield break;
                }
                
                var sizeIncrease = textInfo.lineInfo.Sum(line => line.lineHeight);

                size.y = 40 + sizeIncrease + 5; //cuz the text is at Y = -40
                descriptionPanelTr.sizeDelta = size;
            }
            
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovering = true;
            
            ShowDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovering = false;
            
            StartCoroutine(DelayedPointerExit());
            return;

            IEnumerator DelayedPointerExit()
            {
                yield return new WaitForSeconds(0.01f);
                
                if(abilityShower.IsHovering) yield break;
                HideDescription();
            }
        }
    }
}

