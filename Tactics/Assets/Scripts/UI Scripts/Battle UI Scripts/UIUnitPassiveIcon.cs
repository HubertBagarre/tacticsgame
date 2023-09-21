using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnitPassiveIcon : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI stacksCountText;
        public UnitPassiveInstance PassiveInstance { get; private set; }

        public void LinkToPassive(UnitPassiveInstance instance)
        {
            PassiveInstance = instance;
            image.sprite = PassiveInstance.SO.Sprite;
            PassiveInstance.OnCurrentStacksChanged += UpdateStacksCount;
        }

        private void UpdateStacksCount(int amount)
        {
            stacksCountText.text = PassiveInstance.IsStackable ? $"{amount}" : "";
        }


    }
}


