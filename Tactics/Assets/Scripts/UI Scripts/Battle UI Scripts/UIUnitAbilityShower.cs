using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UIComponent
{
    public class UIUnitAbilityShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIUnitAbilityInfo abilityInfo;
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            // show ability Info
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // check if on ability info
            //if not hide ability info
        }
    }
}


