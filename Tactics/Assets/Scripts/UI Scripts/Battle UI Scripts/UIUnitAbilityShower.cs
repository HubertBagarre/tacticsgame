using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UIComponent
{
    public class UIUnitAbilityShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIUnitAbilityInfo abilityInfo;
        public bool IsHovering { get; private set; }
        
        private void Start()
        {
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


