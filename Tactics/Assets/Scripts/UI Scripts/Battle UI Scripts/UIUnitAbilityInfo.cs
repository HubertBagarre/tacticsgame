using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    using ScriptableObjects.Ability;
    
    public class UIUnitAbilityInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
    {
        [SerializeField] private UIUnitAbilityShower abilityShower;
        
        [Header("Description")]
        [SerializeField] private GameObject descriptionPanelGo;
        [SerializeField] private RectTransform descriptionPanelTr;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        
        private UnitAbilityInstance associatedAbility;
        private Unit associatedUnit;

        [SerializeField] private bool show = true;

        private void Start()
        {
            descriptionPanelGo.SetActive(false);
        }
        
        public void LinkAbility(UnitAbilityInstance ability,Unit caster)
        {
            associatedAbility = ability;
            associatedUnit = caster;
            
            UpdateDescription();
        }
        
        public void ShowDescription(BaseEventData _)
        {
            UIBattleManager.Tooltip.Hide();
            
            UpdateDescription();
            descriptionPanelGo.SetActive(true);
        }
        
        public void HideDescription(BaseEventData _)
        {
            HideDescription();
            
        }
        
        private void HideDescription()
        {
            UIBattleManager.Tooltip.Hide();
            //descriptionPanelGo.SetActive(isHoveringDescription);
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
            // Showdescription
            
            ShowDescription(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // check if on shower info
            //if not hide ability info
            
            HideDescription(eventData);
        }
    }
}

