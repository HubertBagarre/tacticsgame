using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnitAbilityShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIUnitAbilityInfo abilityInfo;
        [SerializeField] private Image abilityImage;
        public AbilityInstance AssociatedAbility { get; private set; }
        public NewUnit AssociatedUnit { get; private set; }
        
        public bool IsHovering { get; private set; }

        private void OnDisable()
        {
            abilityInfo.HideDescription();
            IsHovering = false;
        }

        public void LinkAbility(AbilityInstance ability,NewUnit caster)
        {
            AssociatedAbility = ability;
            AssociatedUnit = caster;

            abilityImage.sprite = AssociatedAbility.SO.Sprite;
            abilityInfo.HideDescription();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovering = true;
            
            abilityInfo.ShowDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovering = false;
            
            StartCoroutine(DelayedPointerExit());
            return;

            IEnumerator DelayedPointerExit()
            {
                yield return new WaitForSeconds(0.01f);
                
                if(abilityInfo.IsHovering) yield break;
                abilityInfo.HideDescription();
            }
        }
    }
}


