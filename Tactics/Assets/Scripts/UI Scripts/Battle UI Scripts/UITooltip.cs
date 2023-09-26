using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UITooltip : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipGo;
        [SerializeField] private RectTransform tooltipTr;
        [SerializeField] private TextMeshProUGUI tooltipText;

        public bool IsEnabled => tooltipGo.activeSelf;

        public void Hide()
        {
            tooltipGo.SetActive(false);
        }

        public void Show(Vector2 position,string text)
        {
            tooltipGo.SetActive(true);

            tooltipTr.position = position;

            tooltipText.text = text;
            
            //update box size, check uiunitabilitybutton for code 
        }
    }
}