using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Battle
{
    using AbilityEvent;
    using BattleEvents;
    using UIEvents;

    public class UIBattleManager : MonoBehaviour
    {
        [Header("Battle Timeline")] [SerializeField]
        private UIBattleEntityTimeline battleEntityTimelinePrefab;

        [SerializeField] private Transform battleTimelineParent;

        [Header("Player Controls")] [SerializeField]
        private Button endTurnButton;

        [SerializeField] private UIUnitAbilityButton abilityButtonPrefab;
        [SerializeField] private Transform abilityButtonParent;
        private List<UIUnitAbilityButton> abilityButtons = new();

        [FormerlySerializedAs("tileSelectionUIObj")]
        [Header("Ability Tile Selection")]
        [SerializeField] private GameObject abilityTargetSelectionUIObj;
        [SerializeField] private Button confirmTileSelectionButton;
        [SerializeField] private Button cancelTileSelectionButton;
        [SerializeField] private TextMeshProUGUI selectionsLeftText;
        private UnitAbilityInstance currentAbilityInTargetSelection;
        
        private Dictionary<BattleEntity, UIBattleEntityTimeline> uibattleTimelineDict = new();

        private void Start()
        {
            AddCallbacks();

            EnableEndTurnButton(false);

            ShowAbilityTargetSelection(false);
        }

        private void AddCallbacks()
        {
            //Timeline events
            EventManager.AddListener<EntityJoinBattleEvent>(InstantiateBattleEntityTimelineUI);
            EventManager.AddListener<EntityLeaveBattleEvent>(RemoveBattleEntityTimelineUI);
            EventManager.AddListener<UpdateTurnValuesEvent>(ReorderBattleEntityTimeline);

            //Player Buttons events
            EventManager.AddListener<StartPlayerControlEvent>(ShowPlayerButtonsOnPlayerTurnStart);
            EventManager.AddListener<EndPlayerControlEvent>(HidePlayerButtonsOnPlayerTurnEnd);

            //EventManager.AddListener<StartAbilitySelectionEvent>();

            //TODO - Move Player Movement Here (?)
            EventManager.AddListener<StartUnitMovementSelectionEvent>(ShowUIForUnitMovement);

            EventManager.AddListener<StartAbilitySelectionEvent>(ShowAbilityTargetSelectionOnTargetSelectionStart);
            EventManager.AddListener<EndAbilitySelectionEvent>(HideAbilityTargetSelectionOnTargetSelectionEnd);
            cancelTileSelectionButton.onClick.AddListener(CancelSelection);
            confirmTileSelectionButton.onClick.AddListener(ConfirmSelection);

            void CancelSelection()
            {
                EventManager.Trigger(new EndAbilitySelectionEvent(true));
            }

            void ConfirmSelection()
            {
                EventManager.Trigger(new EndAbilitySelectionEvent(false));
            }
        }
        
        private void ShowPlayerButtonsOnPlayerTurnStart(StartPlayerControlEvent ctx)
        {
            EnableEndTurnButton(true);

            ShowUnitAbilitiesButton(ctx.PlayerUnit);
        }

        private void HidePlayerButtonsOnPlayerTurnEnd(EndPlayerControlEvent ctx)
        {
            EnableEndTurnButton(false);

            HideUnitAbilitiesButton();
        }

        private void HidePlayerButtonsOnAbilitySelection(StartAbilitySelectionEvent ctx)
        {
            
        }
        
        private void ShowPlayerButtonsOnAbilitySelection(StartAbilitySelectionEvent ctx)
        {
            
        }

        private void EnableEndTurnButton(bool value)
        {
            endTurnButton.interactable = value;
        }

        private void ShowUIForUnitMovement(StartUnitMovementSelectionEvent ctx)
        {
            
        }

        private void HideUIForUnitMovement()
        {
            
        }
        
        #region Ability Target Selection
        
        private void ShowAbilityTargetSelection(bool value)
        {
            abilityTargetSelectionUIObj.SetActive(value);
        }

        private void ShowAbilityTargetSelectionOnTargetSelectionStart(StartAbilitySelectionEvent ctx)
        {
            currentAbilityInTargetSelection = ctx.Ability;

            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated += UpdateAbilitySelectionLeftText;
            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated += UpdateConfirmAbilityTargetSelectionButton;

            UpdateAbilitySelectionLeftText(0);
            UpdateConfirmAbilityTargetSelectionButton(0);
            
            ShowAbilityTargetSelection(!ctx.Ability.SO.IsInstantCast);
        }

        private void UpdateConfirmAbilityTargetSelectionButton(int _)
        {
            confirmTileSelectionButton.interactable = currentAbilityInTargetSelection.SelectionsLeft == 0 || currentAbilityInTargetSelection.ExpectedSelections == 0;
        }

        private void UpdateAbilitySelectionLeftText(int _)
        {
            selectionsLeftText.text = $"Select {currentAbilityInTargetSelection.SelectionsLeft} Target{(currentAbilityInTargetSelection.SelectionsLeft > 0 ? "s":"")}"; //Select 66 Targets
        }

        private void HideAbilityTargetSelectionOnTargetSelectionEnd(EndAbilitySelectionEvent ctx)
        {
            ShowAbilityTargetSelection(false);
            
            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated -= UpdateAbilitySelectionLeftText;
            currentAbilityInTargetSelection.OnCurrentSelectedTilesUpdated -= UpdateConfirmAbilityTargetSelectionButton;
            
            //TODO- Clear selection
        }

        #endregion

        #region Unit Ability

        

        private void ShowUnitAbilitiesButton(Unit unit)
        {
            var abilities = unit.AbilityInstances;

            UpdateAbilityButtonCount(abilities.Count);

            for (var index = 0; index < abilities.Count; index++)
            {
                var ability = abilities[index];
                abilityButtons[index].LinkAbility(ability,unit);
                abilityButtonParent.GetChild(index).gameObject.SetActive(true);
            }
        }

        private void UpdateAbilityButtonCount(int amount)
        {
            var currentButtons = abilityButtonParent.childCount;
            var missingButtons = amount - currentButtons;

            if (missingButtons <= 0) return;

            for (int i = 0; i < missingButtons; i++)
            {
                var ability = Instantiate(abilityButtonPrefab, abilityButtonParent);
                abilityButtons.Add(ability);
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
    }
}