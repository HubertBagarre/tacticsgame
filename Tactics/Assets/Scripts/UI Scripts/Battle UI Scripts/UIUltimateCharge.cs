using UnityEngine;

namespace Battle.UIComponent
{
    public class UIUltimateCharge : MonoBehaviour
    {
        [SerializeField] private GameObject chargedObj;
    
        public void Charge(bool value)
        {
            chargedObj.SetActive(value);
        }
    }
}


