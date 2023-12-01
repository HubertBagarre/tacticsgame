using System.Collections.Generic;
using System.Linq;
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

        [Header("Abilities")]
        [SerializeField] private UIUnitAbilityShower unitAbilityShowerPrefab;
        [SerializeField] private Transform abilityShowerParent;
        private Dictionary<AbilityInstance,GameObject> abilityShowerDict = new();
        
        [Header("Passives")]
        [SerializeField] private UIUnitPassiveIcon unitPassiveIconPrefab;
        [SerializeField] private Transform passiveIconParent;
        private Dictionary<PassiveInstance,GameObject> passiveIconDict = new();
        
        [Header("Stats")]
        [SerializeField] private UIStatElement UIStatElementPrefab;
        private List<UIStatElement> statElements = new();
        private UIStatElement MovementElement => statElements[(int)UnitStat.Movement];
        private UIStatElement SpeedElement => statElements[(int)UnitStat.Speed];
        private UIStatElement AttackElement => statElements[(int)UnitStat.Attack];
        private UIStatElement AttackRangeElement => statElements[(int)UnitStat.AttackRange];
        private UIStatElement HpElement => statElements[(int)UnitStat.Hp];

        private NewUnit currentDisplayingUnit;

        [SerializeField] private Transform statElementParent;
        
        public void DisplayUnitTooltip(NewUnit unit)
        {
            RemoveCallbacks(currentDisplayingUnit);
            HideCurrentStats();
            HideCurrentAbilities();
            HideCurrentPassives();
            
            currentDisplayingUnit = unit;
            AddCallbacks(currentDisplayingUnit);
            
            ShowUnitInfo();
            ShowCurrentStats();
            ShowCurrentAbilities();
            UpdateCurrentPassives();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void HideCurrentAbilities()
        {
            if (currentDisplayingUnit == null) return;
            
            foreach (var abilityInstance in currentDisplayingUnit.AbilityInstances)
            {
                if (abilityShowerDict.ContainsKey(abilityInstance))
                {
                    abilityShowerDict[abilityInstance].SetActive(false);
                }
            }
        }

        private void HideCurrentPassives()
        {
            if (currentDisplayingUnit == null) return;
            
            foreach (var passiveInstance in currentDisplayingUnit.PassiveInstances)
            {
                if (passiveIconDict.ContainsKey(passiveInstance))
                {
                    passiveIconDict[passiveInstance].SetActive(false);
                }
            }
        }

        private void HideCurrentStats()
        {
            if (currentDisplayingUnit == null) return;
            
            foreach (var statElement in statElements)
            {
                statElement.Show(false);
            }
        }

        private void ShowCurrentAbilities()
        {
            foreach (var abilityInstance in currentDisplayingUnit.AbilityInstances.Where(abilityInstance => abilityInstance.ShowInTooltip))
            {
                if (!abilityShowerDict.ContainsKey(abilityInstance))
                {
                    var shower = Instantiate(unitAbilityShowerPrefab, abilityShowerParent);
                    shower.LinkAbility(abilityInstance,currentDisplayingUnit);
                    
                    abilityShowerDict.Add(abilityInstance,shower.gameObject);
                }
                
                abilityShowerDict[abilityInstance].SetActive(true);
            }
        }

        private void AddPassive(PassiveInstance.AddPassiveBattleAction action)
        {
            var passiveInstance = action.PassiveInstance;
            
            if(currentDisplayingUnit != passiveInstance.Container) return;
            
            if (!passiveIconDict.ContainsKey(passiveInstance))
            {
                var icon = Instantiate(unitPassiveIconPrefab, passiveIconParent);
                icon.LinkToPassive(passiveInstance);
                    
                passiveIconDict.Add(passiveInstance,icon.gameObject);
            }
            
            UpdateCurrentPassives();
        }

        private void RemovePassive(PassiveInstance.RemovePassiveBattleAction action)
        {
            var passiveInstance = action.PassiveInstance;
            
            if(currentDisplayingUnit !=  passiveInstance.Container) return;
            
            if (passiveIconDict.ContainsKey(passiveInstance))
            {
                var icon = passiveIconDict[passiveInstance];
                passiveIconDict.Remove(passiveInstance);
                Destroy(icon);
            }
            
            UpdateCurrentPassives();
        }

        private void UpdateCurrentPassives()
        {
            foreach (var passiveInstance in currentDisplayingUnit.PassiveInstances)
            {
                if (!passiveIconDict.ContainsKey(passiveInstance))
                {
                    var icon = Instantiate(unitPassiveIconPrefab, passiveIconParent);
                    icon.LinkToPassive(passiveInstance);
                    
                    passiveIconDict.Add(passiveInstance,icon.gameObject);
                }
                
                passiveIconDict[passiveInstance].SetActive(true);
            }
        }

        private void ShowUnitInfo()
        {
            if (currentDisplayingUnit == null) return;
            var so = currentDisplayingUnit.Stats.So;
            var color = currentDisplayingUnit.Team == 0 ? "<color=#00ffffff>" : "<color=red>";
            unitNameText.text = $"{color}{so.Name}</color>";
            unitTitleText.text = "";
            unitPortraitImage.sprite = so.Portrait;
        }

        private void ShowCurrentStats()
        {
            if (currentDisplayingUnit == null) return;
            
            UpdateStatElements(currentDisplayingUnit);
            
            foreach (var statElement in statElements)
            {
                statElement.Show(true);
            }
        }

        public void SetupStatElement()
        {
            statElements.Clear();
            abilityShowerDict.Clear();

            for (var i = 0; i < 5; i++)
            {
                statElements.Add(Instantiate(UIStatElementPrefab, statElementParent));
            }
            
            HpElement.ChangeStatText("Hp  :");
            AttackElement.ChangeStatText("Atk :");
            AttackRangeElement.ChangeStatText("AtkR:");
            MovementElement.ChangeStatText("Mov :");
            SpeedElement.ChangeStatText("Spd :");
        }

        private void AddCallbacks(NewUnit unit)
        {
            if(unit == null) return;
            var statsInstance = unit.Stats;
            statsInstance.OnMaxHpModified += UpdateHpStatElement;
            statsInstance.OnCurrentHpModified += UpdateHpStatElement;
            statsInstance.OnAttackModified += UpdateAttackStatElement;
            statsInstance.OnAttackRangeModified += UpdateAttackRangeStatElement;
            statsInstance.OnMovementModified += UpdateMovementStatElement;
            statsInstance.OnSpeedModified += UpdateSpeedStatElement;
            statsInstance.OnCurrentMovementModified += UpdateMovementStatElement;
            
            ActionEndInvoker<PassiveInstance.AddPassiveBattleAction>.OnInvoked += AddPassive;
            ActionEndInvoker<PassiveInstance.RemovePassiveBattleAction>.OnInvoked += RemovePassive;
        }

        private void RemoveCallbacks(NewUnit unit)
        {
            if(unit == null) return;
            var statsInstance = unit.Stats;
            statsInstance.OnMaxHpModified -= UpdateHpStatElement;
            statsInstance.OnCurrentHpModified -= UpdateHpStatElement;
            statsInstance.OnAttackModified -= UpdateAttackStatElement;
            statsInstance.OnAttackRangeModified -= UpdateAttackRangeStatElement;
            statsInstance.OnMovementModified -= UpdateMovementStatElement;
            statsInstance.OnSpeedModified -= UpdateSpeedStatElement;
            statsInstance.OnCurrentMovementModified -= UpdateMovementStatElement;
            
            ActionEndInvoker<PassiveInstance.AddPassiveBattleAction>.OnInvoked -= AddPassive;
            ActionEndInvoker<PassiveInstance.RemovePassiveBattleAction>.OnInvoked -= RemovePassive;
        }

        private void UpdateHpStatElement(UnitStatsInstance statsInstance)
        {
            var hasDif = statsInstance.MaxHpDiff != 0;
            if (hasDif)
            {
                var color = statsInstance.MaxHpDiff > 0 ? "<color=green>" : "<color=red>";
                HpElement.ChangeValueText($"{statsInstance.CurrentHp}/{color}{statsInstance.MaxHp}</color>");
                return;
            }

            HpElement.ChangeValueText($"{statsInstance.CurrentHp}/{statsInstance.MaxHp}");
        }

        private void UpdateAttackStatElement(UnitStatsInstance statsInstance)
        {
            var hasDif = statsInstance.AttackDiff != 0;
            if (hasDif)
            {
                var color = statsInstance.AttackDiff > 0 ? "<color=green>" : "<color=red>";
                AttackElement.ChangeValueText($"{color}{statsInstance.Attack}</color>");
                return;
            }

            AttackElement.ChangeValueText($"{statsInstance.Attack}");
        }

        private void UpdateAttackRangeStatElement(UnitStatsInstance statsInstance)
        {
            var hasDif = statsInstance.AttackRangeDiff != 0;
            if (hasDif)
            {
                var color = statsInstance.AttackRangeDiff > 0 ? "<color=green>" : "<color=red>";
                AttackRangeElement.ChangeValueText($"{color}{statsInstance.AttackRange}</color>");
                return;
            }

            AttackRangeElement.ChangeValueText($"{statsInstance.AttackRange}");
        }

        private void UpdateMovementStatElement(UnitStatsInstance statsInstance)
        {
            var hasDif = statsInstance.MovementDiff != 0;
            if (hasDif)
            {
                var color = statsInstance.MovementDiff > 0 ? "<color=green>" : "<color=red>";
                MovementElement.ChangeValueText(
                    $"{statsInstance.CurrentMovement}/{color}{statsInstance.Movement}</color>");
                return;
            }

            MovementElement.ChangeValueText($"{statsInstance.CurrentMovement}/{statsInstance.Movement}");
        }
        
        private void UpdateSpeedStatElement(UnitStatsInstance statsInstance)
        {
            var hasDif = statsInstance.SpeedDiff != 0;
            if (hasDif)
            {
                var color = statsInstance.SpeedDiff > 0 ? "<color=green>" : "<color=red>";
                SpeedElement.ChangeValueText($"{color}{statsInstance.Speed}</color>");
                return;
            }

            SpeedElement.ChangeValueText($"{statsInstance.Speed}");
        }

        private void UpdateStatElements(NewUnit unit)
        {
            var statsInstance = unit.Stats;
            UpdateHpStatElement(statsInstance);
            UpdateAttackStatElement(statsInstance);
            UpdateAttackRangeStatElement(statsInstance);
            UpdateMovementStatElement(statsInstance);
            UpdateSpeedStatElement(statsInstance);
        }
    }
}