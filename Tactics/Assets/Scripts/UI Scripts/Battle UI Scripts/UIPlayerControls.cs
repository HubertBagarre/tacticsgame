using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.UIComponent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerControls : MonoBehaviour
{
        [SerializeField] private GameObject parent;
        
        /*
        [Header("Battle State")]
        [SerializeField] private RectTransform battleRoundIndicatorTr;
        [SerializeField] private TextMeshProUGUI battleRoundIndicatorText;
        [SerializeField] private RectTransform battleStartIndicatorTr;*/

        /*
        [Header("Unit UI")]
        [SerializeField] private UIUnit uiUnitPrefab;
        private Dictionary<Unit, UIUnit> uiUnitsDict = new ();
        
        [Header("Unit Tooltip")]
        [SerializeField] private UIUnitTooltip unitTooltip;
        */
                
        [Header("Player Controls")]
        [SerializeField] private Button endTurnButton;
        
        /*
        [Header("Ability Buttons")]
        [SerializeField] private UIUnitAbilityButton abilityButtonPrefab;
        [SerializeField] private Transform abilityButtonParent;
        private List<UIUnitAbilityButton> abilityButtons = new();
        private Dictionary<Unit, List<UIUnitAbilityButton>> unitAbilitButtonsDict = new();
        

        [Header("Tooltip")]
        [SerializeField] private UITooltip tooltip;
        public static UITooltip Tooltip { get; private set; }
        */
        
        [Header("Ability Tile Selection")]
        [SerializeField] private GameObject abilityTargetSelectionUIObj;
        [SerializeField] private Button confirmTileSelectionButton;
        [SerializeField] private Button cancelTileSelectionButton;
        [SerializeField] private TextMeshProUGUI selectionsLeftText;
        private UnitAbilityInstance currentAbilityInTargetSelection;

        [Header("Ability Points")]
        [SerializeField] private UIAbilityPointCharge abilityPointChargePrefab;
        [SerializeField] private Transform abilityPointChargeParent;
        private UIAbilityPointCharge[] abilityPointCharges;
        
        private UnitTurnBattleAction currentTurnUnitAction;

        private void Start()
        { 
                HidePlayerUI();
                
                AddCallbacks();
        }
        
        private void AddCallbacks()
        {
                ActionStartInvoker<UnitTurnBattleAction>.OnInvoked += TryShowPlayerUI;
                ActionEndInvoker<UnitTurnBattleAction>.OnInvoked += HidePlayerUI;
        }

        private void RemoveCallbacks()
        {
                ActionStartInvoker<UnitTurnBattleAction>.OnInvoked -= TryShowPlayerUI;
                ActionEndInvoker<UnitTurnBattleAction>.OnInvoked -= HidePlayerUI;
        }
        
        private void TryShowPlayerUI(UnitTurnBattleAction action)
        {
                currentTurnUnitAction = action;
                
                if(!currentTurnUnitAction.IsPlayerTurn) return;

                ShowPlayerUI();
        }

        private void ShowPlayerUI()
        {
                parent.SetActive(true);
                abilityTargetSelectionUIObj.SetActive(false);
                
                endTurnButton.onClick.AddListener(currentTurnUnitAction.RequestEndTurn);
        }

        private void HidePlayerUI(UnitTurnBattleAction _ = null)
        {
                if (currentTurnUnitAction != null)
                {
                        endTurnButton.onClick.RemoveListener(currentTurnUnitAction.RequestEndTurn);
                }
                
                currentTurnUnitAction = null;
                parent.SetActive(false);
        }
}
