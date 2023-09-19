using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UIAbilityPointCharge : MonoBehaviour
    {
        [SerializeField] private GameObject chargedObj;
        private bool charged = false;

        public void Start()
        {
            chargedObj.SetActive(false);
        }

        public void Charge(bool value)
        {
            if (charged && !value)
            {
                //play uncharge animation
                charged = false;
                chargedObj.SetActive(false);
            }

            if (!charged && value)
            {
                //play charge animation
                charged = true;
                chargedObj.SetActive(true);
            }
        }
    }
}

