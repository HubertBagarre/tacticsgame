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

            ShowAbilityTargetSelectionButtons(false);
        }

        private void AddCallbacks()
        {
            //Timeline events
            EventManager.AddListener<EntityJoinBattleEvent>(InstantiateBattleEntityTimelineUI);
            EventManager.AddListener<EntityLeaveBattleEvent>(RemoveBattleEntityTimelineUI);
            EventManager.AddListener<UpdateTurnValuesEvent>(ReorderBattleEntityTimeline);

            //Player Abilities Buttons events
            EventManager.AddListener<StartPlayerControlEvent>(ShowPlayerButtonsOnPlayerTurnStart);
            EventManager.AddListener<EndPlayerControlEvent>(HidePlayerButtonsOnPlayerTurnEnd);
            EventManager.AddListener<StartAbilityCastEvent>(HideAbilityButtonsOnAbilityCast);

            EventManager.AddListener<StartUnitMovementSelectionEvent>(ShowWalkableTiles);

            AbilityManager.OnUpdatedCastingAbility += UpdateAbilityTargetSelection;
            
            cancelTileSelectionButton.onClick.AddListener(CancelSelection);
            confirmTileSelectionButton.onClick.AddListener(ConfirmSelection);

            void CancelSelection()
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
            }

            void ConfirmSelection()
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
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

        private void HideAbilityButtonsOnAbilityCast(StartAbilityCastEvent ctx)
        {
            EnableEndTurnButton(false);

            HideUnitAbilitiesButton();
            
            if(ctx.Ability.SO.EndUnitTurnAfterCast) return;
                
            EventManager.AddListener<EndAbilityCastEvent>(ShowAbilityButtonsAfterAbilityCast,true);
            
            void ShowAbilityButtonsAfterAbilityCast(EndAbilityCastEvent endAbilityCastEvent)
            {
                EnableEndTurnButton(true);

                ShowUnitAbilitiesButton(ctx.Caster);
            }
        }

        private void EnableEndTurnButton(bool value)
        {
            endTurnButton.interactable = value;
        }

        private void ShowWalkableTiles(StartUnitMovementSelectionEvent ctx)
        {
            foreach (var tile in ctx.SelectableTiles)
            {
                tile.SetAppearance(Tile.Appearance.Selectable);
            }
        }
        
        #region Ability Target Selection
        
        private void ShowAbilityTargetSelectionButtons(bool value)
        {
            abilityTargetSelectionUIObj.SetActive(value);
        }

        private void UpdateAbilityTargetSelection(Unit _,UnitAbilityInstance ability)
        {
            if (ability == null)
            {
                ShowAbilityTargetSelectionButtons(false);
            
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
            
            ShowAbilityTargetSelectionButtons(!currentAbilityInTargetSelection.SO.SkipTargetSelection);
        }

        private void UpdateConfirmAbilityTargetSelectionButton(int _)
        {
            confirmTileSelectionButton.interactable = currentAbilityInTargetSelection.SelectionsLeft == 0 || currentAbilityInTargetSelection.ExpectedSelections == 0;
        }

        private void UpdateAbilitySelectionLeftText(int _)
        {
            selectionsLeftText.text = $"Select {currentAbilityInTargetSelection.SelectionsLeft} Target{(currentAbilityInTargetSelection.SelectionsLeft > 0 ? "s":"")}"; //Select 66 Targets
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