using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UIComponent
{
    public class UIUnitTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitTitleText;
        [SerializeField] private Image unitPortraitImage;
        
        [SerializeField] private UIStatElement UIStatElementPrefab;
        private Dictionary<Unit, List<UIStatElement>> unitStatElements = new ();
        private Unit currentDisplayingUnit;

        [SerializeField] private Transform statElementParent;

        private void Start()
        {
            unitStatElements.Clear();
            Hide();
        }

        public void DisplayUnitTooltip(Unit unit)
        {
            HideCurrentStats();
            currentDisplayingUnit = unit;
            ShowUnitInfo();
            ShowCurrentStats();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void HideCurrentStats()
        {
            if(currentDisplayingUnit == null) return;
            if(!unitStatElements.ContainsKey(currentDisplayingUnit)) return;

            foreach (var statElement in unitStatElements[currentDisplayingUnit])
            {
                statElement.Show(false);
            }
        }

        private void ShowUnitInfo()
        {
            if(currentDisplayingUnit == null) return;
            var so = currentDisplayingUnit.Stats.StatsSo;
            unitNameText.text = so.Name;
            unitTitleText.text = "";
            unitPortraitImage.sprite = so.Portrait;
        }
        
        private void ShowCurrentStats()
        {
            if(currentDisplayingUnit == null) return;
            if (!unitStatElements.ContainsKey(currentDisplayingUnit))
            {
                var unitStat = currentDisplayingUnit.Stats;
                unitStatElements.Add(currentDisplayingUnit,new List<UIStatElement>());
                var list = unitStatElements[currentDisplayingUnit];
                for (var i = 0; i < 7; i++)
                {
                    list.Add(Instantiate(UIStatElementPrefab,statElementParent));
                }

                var hpElement = list[0];
                var attackElement = list[1];
                var attackRangeElement = list[2];
                var movementElement = list[3];
                var speedElement = list[4];
                
                SetupStatElement(unitStat);

                void SetupStatElement(UnitStatsInstance statsInstance)
                {
                    hpElement.ChangeStatText("Hp  :");
                    attackElement.ChangeStatText("Atk :");
                    attackRangeElement.ChangeStatText("AtkR:");
                    movementElement.ChangeStatText("Mov :");
                    speedElement.ChangeStatText("Spd :");

                    statsInstance.OnMaxHpModified += UpdateStatElements;
                    statsInstance.OnCurrentHpModified += UpdateStatElements;
                    statsInstance.OnAttackModified += UpdateStatElements;
                    statsInstance.OnAttackRangeModified += UpdateStatElements;
                    statsInstance.OnMovementModified += UpdateStatElements;
                    statsInstance.OnSpeedModified += UpdateStatElements;
                    
                    currentDisplayingUnit.OnMovementLeftChanged += UpdateUnitStatElementsForMovement;

                    UpdateStatElements(statsInstance);

                    void UpdateUnitStatElementsForMovement(int _)
                    {
                        UpdateStatElements(statsInstance);
                    }
                }
                
                void UpdateStatElements(UnitStatsInstance statsInstance)
                {
                    hpElement.ChangeValueText($"{statsInstance.CurrentHp}/{statsInstance.MaxHp}");
                    attackElement.ChangeValueText($"{statsInstance.Attack}");
                    attackRangeElement.ChangeValueText($"{statsInstance.AttackRange}");
                    movementElement.ChangeValueText($"{currentDisplayingUnit.MovementLeft}/{statsInstance.Movement}");
                    speedElement.ChangeValueText($"{statsInstance.Speed}");
                }
            }

            foreach (var statElement in unitStatElements[currentDisplayingUnit])
            {
                statElement.Show(true);
            }
        }
    }

}

