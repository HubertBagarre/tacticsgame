using UnityEngine;

namespace Battle.UIComponent
{
    public class UIAbilityCostCharge : MonoBehaviour
    {
        // Test a version with only one gem with cost on it
        [SerializeField] private GameObject positiveCostObj;
        [SerializeField] private GameObject negativeCostObj;

        public void SetCost(int amount)
        {
            //if the amount is NEGATIVE, you will GAIN Ability Points
            positiveCostObj.SetActive(amount<0);
            negativeCostObj.SetActive(amount>0);
        }
    }
}


