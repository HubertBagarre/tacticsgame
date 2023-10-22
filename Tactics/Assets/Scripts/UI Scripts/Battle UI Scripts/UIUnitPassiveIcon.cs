using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnitPassiveIcon : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI stacksCountText;
        public PassiveInstance<Unit>  PassiveInstance { get; private set; }

        public void LinkToPassive(PassiveInstance<Unit>  instance)
        {
            PassiveInstance = instance;
            image.sprite = PassiveInstance.SO.Sprite;
            PassiveInstance.OnCurrentStacksChanged += UpdateStacksCount;
            
            UpdateStacksCount(PassiveInstance.CurrentStacks);
        }

        private void UpdateStacksCount(int amount)
        {
            stacksCountText.text = PassiveInstance.IsStackable ? $"{amount}" : "";
        }


    }
}


