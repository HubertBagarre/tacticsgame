using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIPointerEvents : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] private UnityEvent<PointerEventData> clickEvents = new(); 

    public void OnPointerClick(PointerEventData eventData)
    {
        clickEvents.Invoke(eventData);
    }
}
