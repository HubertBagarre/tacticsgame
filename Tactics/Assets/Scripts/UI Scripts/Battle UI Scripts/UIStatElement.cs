using TMPro;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UIStatElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private TextMeshProUGUI valueText;

        public void Show(bool value)
        {
            gameObject.SetActive(value);
        }
        
        public void ChangeStatText(string text)
        {
            statText.text = text;
        }

        public void ChangeValueText(string text)
        {
            valueText.text = text;
        }
    }
}


