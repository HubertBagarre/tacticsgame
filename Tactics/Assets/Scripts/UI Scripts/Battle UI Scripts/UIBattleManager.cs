using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    using AbilityEvents;
    using ScriptableObjects.Ability;
    using BattleEvents;
    using UIEvents;
    using UIComponent;
    using InputEvent;

    public class UIBattleManager : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        
        [Header("Battle Timeline")] [SerializeField]
        private UIBattleEntityTimeline battleEntityTimelinePrefab;
        [SerializeField] private Transform battleTimelineParent;
        
        [Header("Battle State")]
        [SerializeField] private RectTransform battleRoundIndicatorTr;
        [SerializeField] private TextMeshProUGUI battleRoundIndicatorText;
        [SerializeField] private RectTransform battleStartIndicatorTr;

        [Header("Unit Tooltip")]
        [SerializeField] private UIUnitTooltip unitTooltip;
        
        [Header("Player Controls")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private UIUnitAbilityButton abilityButtonPrefab;
        [SerializeField] private Transform abilityButtonParent;
        private List<UIUnitAbilityButton> abilityButtons = new();
        private Dictionary<Unit, List<UIUnitAbilityButton>> unitAbilitButtonsDict = new();

        [Header("Tooltip")]
        [SerializeField] private UITooltip tooltip;
        public static UITooltip Tooltip { get; private set; }
        
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
        
        private Dictionary<BattleEntity, UIBattleEntityTimeline> uibattleTimelineDict = new();
        public static event Action OnEndTurnButtonClicked;
        
        private void Start()
        {
            AssignTooltip();
            
            AddCallbacks();

            battleTimelineParent.gameObject.SetActive(false);
            
            battleStartIndicatorTr.anchoredPosition = new Vector2(-Screen.width, battleStartIndicatorTr.anchoredPosition.y);
            battleRoundIndicatorTr.anchoredPosition = new Vector2(-Screen.width, battleRoundIndicatorTr.anchoredPosition.y);

            ShowEndTurnButton(false);

            ShowAbilityTargetSelectionButtons(false);
            
            InstantiateAbilityPointCharges();
        }

        private void AddCallbacks()
        {
            //Timeline events
            EventManager.AddListener<EntityJoinBattleEvent>(InstantiateBattleEntityTimelineUI);
            EventManager.AddListener<EntityLeaveBattleEvent>(RemoveBattleEntityTimelineUI);
            EventManager.AddListener<UpdateTurnValuesEvent>(ReorderBattleEntityTimeline);

            //Unit Tooltip
            EventManager.AddListener<ClickUnitEvent>(ShowUnitTooltip);
            
            //Player Abilities Buttons events
            EventManager.AddListener<StartPlayerControlEvent>(ShowPlayerButtonsOnPlayerTurnStart);
            EventManager.AddListener<EndPlayerControlEvent>(HidePlayerButtonsOnPlayerTurnEnd);
            EventManager.AddListener<EndAbilityTargetSelectionEvent>(ShowEndTurnButtonOnAbilityTargetSelectionCancel);
            EventManager.AddListener<StartAbilityCastEvent>(HidePlayerButtonsOnAbilityCast);
            //EventManager.AddListener<StartAbilityCastEvent> is in HidePlayerButtonsOnAbilityCast()
            EventManager.AddListener<EndBattleEvent>(HidePlayerButtonsOnBattleEnd);
            
            AbilityManager.OnUpdatedCastingAbility += UpdateAbilityTargetSelection;
            AbilityManager.OnUpdatedAbilityPoints += UpdateAbilityPointCharges;
            
            cancelTileSelectionButton.onClick.AddListener(CancelSelection);
            confirmTileSelectionButton.onClick.AddListener(ConfirmSelection);
            
            //Battle Phases
            endTurnButton.onClick.AddListener(ClickEndTurnButton);

            battleManager.OnStartRound += PlayRoundStartAnimation;
            EventManager.AddListener<StartBattleEvent>(PlayBattleStartAnimation);

            void ClickEndTurnButton()
            {
                OnEndTurnButtonClicked?.Invoke();
                OnEndTurnButtonClicked = null;
            }

            void CancelSelection()
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
            }

            void ConfirmSelection()
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }

            void ShowEndTurnButtonOnAbilityTargetSelectionCancel(EndAbilityTargetSelectionEvent ctx)
            {
                if(ctx.Canceled) ShowEndTurnButton(true);
            }
        }
        
        private void ShowUnitTooltip(ClickUnitEvent ctx)
        {
            var unit = ctx.Unit;
            if (unit == null)
            {
                unitTooltip.Hide();
            }
            
            unitTooltip.DisplayUnitTooltip(ctx.Unit);
            unitTooltip.Show();
        }
        
        private void ShowPlayerButtonsOnPlayerTurnStart(StartPlayerControlEvent ctx)
        {
            ShowEndTurnButton(true);
            
            EnableEndTurnButton(true);

            ShowUnitAbilitiesButton(ctx.PlayerUnit);
        }

        private void HidePlayerButtonsOnPlayerTurnEnd(EndPlayerControlEvent ctx)
        {
            ShowEndTurnButton(false);
            
            EnableEndTurnButton(false);

            HideUnitAbilitiesButton();
        }

        private void HidePlayerButtonsOnBattleEnd(EndBattleEvent ctx)
        {
            Debug.Log("Battle Ended, hiding ui");
            
            ShowEndTurnButton(false);
            
            HideUnitAbilitiesButton();
        }

        private void HidePlayerButtonsOnAbilityCast(StartAbilityCastEvent ctx)
        {
            ShowEndTurnButton(false);

            HideUnitAbilitiesButton();
            
            if(ctx.Ability.SO.EndUnitTurnAfterCast) return;
                
            EventManager.AddListener<EndAbilityCastEvent>(ShowAbilityButtonsAfterAbilityCast,true);
            
            void ShowAbilityButtonsAfterAbilityCast(EndAbilityCastEvent endAbilityCastEvent)
            {
                ShowEndTurnButton(true);
                
                EnableEndTurnButton(true);

                ShowUnitAbilitiesButton(ctx.Caster);
            }
        }
        
        private void ShowEndTurnButton(bool value)
        {
            endTurnButton.gameObject.SetActive(value);
        }

        private void EnableEndTurnButton(bool value)
        {
            endTurnButton.interactable = value;
        }
        
        #region Battle Phases

        private void PlayBattleStartAnimation(StartBattleEvent ctx)
        {
            var posY = battleStartIndicatorTr.anchoredPosition.y;
            var duration = ctx.TransitionDuration;
            
            var sequence = DOTween.Sequence();
            sequence.Append(battleStartIndicatorTr.DOMoveX(0, duration.x));
            sequence.AppendInterval(duration.y);
            sequence.Append(battleStartIndicatorTr.DOMoveX(Screen.width, duration.z));
            sequence.AppendCallback(()=>battleStartIndicatorTr.anchoredPosition = new Vector2(-Screen.width,posY));
            sequence.AppendCallback(() => battleTimelineParent.gameObject.SetActive(true)); //replace with animation

            sequence.Play();
        }

        private void PlayRoundStartAnimation(Vector3 durations)
        {
            battleRoundIndicatorText.text = $"Round {battleManager.CurrentRound}";

            var sequence = DOTween.Sequence();
            sequence.Append(battleRoundIndicatorTr.DOMoveX(0,durations.x));
            sequence.AppendInterval(durations.y);
            sequence.Append(battleRoundIndicatorTr.DOMoveX(Screen.width,durations.z));
            sequence.AppendCallback(()=>battleRoundIndicatorTr.anchoredPosition = new Vector2(-Screen.width,0));

            sequence.Play();
        }
        
        #endregion
        
        #region Ability Target Selection
        
        private void ShowAbilityTargetSelectionButtons(bool value)
        {
            abilityTargetSelectionUIObj.SetActive(value);
            
            if(value) ShowEndTurnButton(false);
        }

        private void UpdateAbilityTargetSelection(Unit _,UnitAbilityInstance ability)
        {
            if (ability == null)
            {
                ShowAbilityTargetSelectionButtons(false);

                if (currentAbilityInTargetSelection == null) return;
                
                currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated -= UpdateAbilitySelectionLeftText;
                currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated -= UpdateConfirmAbilityTargetSelectionButton;
                
                currentAbilityInTargetSelection = null;
                return;
            }
            
            currentAbilityInTargetSelection = ability;

            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated += UpdateAbilitySelectionLeftText;
            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated += UpdateConfirmAbilityTargetSelectionButton;

            UpdateAbilitySelectionLeftText(0);
            UpdateConfirmAbilityTargetSelectionButton(0);
            
            ShowAbilityTargetSelectionButtons(!currentAbilityInTargetSelection.SO.SkipTargetConfirmation);
            //ShowAbilityTargetSelectionButtons(!currentAbilityInTargetSelection.SO.SkipTargetSelection);
        }

        private void UpdateConfirmAbilityTargetSelectionButton(int _)
        {
            if(currentAbilityInTargetSelection == null) return;
            confirmTileSelectionButton.interactable = currentAbilityInTargetSelection.SelectionsLeft == 0 ||
                                                      currentAbilityInTargetSelection.ExpectedSelections == 0;
        }

        private void UpdateAbilitySelectionLeftText(int _)
        {
            if(currentAbilityInTargetSelection == null) return;
            selectionsLeftText.text = $"Select {currentAbilityInTargetSelection.SelectionsLeft} Target{(currentAbilityInTargetSelection.SelectionsLeft > 0 ? "s":"")}"; //Select 66 Targets
        }
        
        #endregion

        #region Unit Ability
        
        private void ShowUnitAbilitiesButton(Unit unit)
        {
            if (!unitAbilitButtonsDict.ContainsKey(unit))
            {
                var abilities = unit.AbilityInstances;
                var dictValue = new List<UIUnitAbilityButton>();
                unitAbilitButtonsDict.Add(unit,dictValue);
                
                foreach (var abilityInstance in abilities)
                {
                    var abilityButton = Instantiate(abilityButtonPrefab, abilityButtonParent);
                    abilityButton.LinkAbility(abilityInstance,unit);
                    abilityButtons.Add(abilityButton);
                    dictValue.Add(abilityButton);
                }
            }

            foreach (var abilityButton in unitAbilitButtonsDict[unit])
            {
                abilityButton.gameObject.SetActive(true);
                abilityButton.UpdateAppearance();
            }
        }
        private void HideUnitAbilitiesButton()
        {
            foreach (var abilityButton in abilityButtons)
            {
                abilityButton.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Battle Timeline

        private void InstantiateBattleEntityTimelineUI(EntityJoinBattleEvent ctx)
        {
            var entity = ctx.Entity;
            
            var ui = Instantiate(battleEntityTimelinePrefab, battleTimelineParent);

            uibattleTimelineDict.Add(entity, ui);

            ui.ConnectToEntity(entity);

            ui.SetPreview(ctx.Preview);
        }

        private void RemoveBattleEntityTimelineUI(EntityLeaveBattleEvent ctx)
        {
            var entity = ctx.Entity;

            if (!uibattleTimelineDict.ContainsKey(entity)) return;

            var ui = uibattleTimelineDict[entity];

            uibattleTimelineDict.Remove(entity);

            ui.Disconnect();

            Destroy(ui.gameObject);
        }

        private void ReorderBattleEntityTimeline(UpdateTurnValuesEvent ctx)
        {
            var order = ctx.EntityTurnOrder;
            var roundIndex = ctx.RoundEndIndex;

            for (int i = 0; i < order.Count; i++)
            {
                var entity = order[i];
                var ui = uibattleTimelineDict[entity];

                ui.transform.SetSiblingIndex(0);

                ui.Show(i <= roundIndex);
            }
        }

        #endregion

        #region Ability Points

        private void InstantiateAbilityPointCharges()
        {
            abilityPointCharges = new UIAbilityPointCharge[AbilityManager.MaxAbilityPoints];

            for (int i = 0; i < AbilityManager.MaxAbilityPoints; i++)
            {
                abilityPointCharges[i] = Instantiate(abilityPointChargePrefab, abilityPointChargeParent);
                abilityPointCharges[i].Charge(false);
            }
        }

        private void UpdateAbilityPointCharges(int previousPoints,int newPoints)
        {
            if(previousPoints == newPoints) return;
            var charge = previousPoints < newPoints;

            if (charge)
            {
                for (int i = previousPoints; i < newPoints; i++)
                {
                    abilityPointCharges[i].Charge(true);
                }
            }
            else
            {
                for (int i = previousPoints - 1; i >= newPoints; i--)
                {
                    abilityPointCharges[i].Charge(false);
                }
            }
        }

        #endregion

        #region Tooltip

        private void AssignTooltip()
        {
            Tooltip = tooltip;
            Tooltip.Hide();
        }
        
        #endregion
    }
}