using System.Collections.Generic;
using UnityEngine;
using Battle.BattleEvents;
using Battle.UIEvents;
using UnityEngine.UI;

namespace Battle
{
    public class UIBattleManager : MonoBehaviour
{
    [Header("Battle Timeline")] [SerializeField]
    private UIBattleEntityTimeline battleEntityTimelinePrefab;

    [SerializeField] private Transform battleTimelineParent;

    [Header("Player Controls")] 
    [SerializeField] private Button endTurnButton;
    
    [SerializeField] private UIUnitAbilityButton abilityButtonPrefab;
    [SerializeField] private Transform abilityButtonParent;
    private List<UIUnitAbilityButton> abilityButtons = new();

    private Dictionary<BattleEntity, UIBattleEntityTimeline> uibattleTimelineDict = new();

    private void Start()
    {
        //Timeline events
        EventManager.AddListener<EntityJoinBattleEvent>(InstantiateBattleEntityTimelineUI);
        EventManager.AddListener<EntityLeaveBattleEvent>(RemoveBattleEntityTimelineUI);
        EventManager.AddListener<UpdateTurnValuesEvent>(ReorderBattleEntityTimeline);

        //Player Buttons events
        EventManager.AddListener<StartPlayerControlEvent>(ShowPlayerButtonsOnPlayerTurnStart);
        EventManager.AddListener<EndPlayerControlEvent>(HidePlayerButtonsOnPlayerTurnEnd);
        
        //TODO - Move Player Movement Here (?)
        
        EnablePlayerButtons(false);
    }


    private void ShowPlayerButtonsOnPlayerTurnStart(StartPlayerControlEvent ctx)
    {
        EnablePlayerButtons(true);
        
        ShowUnitAbilitiesButton(ctx.PlayerUnit);
    }

    private void HidePlayerButtonsOnPlayerTurnEnd(EndPlayerControlEvent ctx)
    {
        EnablePlayerButtons(false);
        
        HideUnitAbilitiesButton();
    }
    
    private void EnablePlayerButtons(bool value)
    {
        endTurnButton.interactable = value;
    }

    #region Unit Ability

    private void ShowUnitAbilitiesButton(Unit unit)
    {
        var abilities = unit.Stats.Abilities;
        
        UpdateAbilityButtonCount(abilities.Count);

        for (var index = 0; index < abilities.Count; index++)
        {
            var ability = abilities[index];
            abilityButtons[index].LinkAbility(ability);
            abilityButtonParent.GetChild(index).gameObject.SetActive(true);
        }
    }

    private void UpdateAbilityButtonCount(int amount)
    {
        var currentButtons = abilityButtonParent.childCount;
        var missingButtons = amount - currentButtons;
        
        if(missingButtons <= 0) return;

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

