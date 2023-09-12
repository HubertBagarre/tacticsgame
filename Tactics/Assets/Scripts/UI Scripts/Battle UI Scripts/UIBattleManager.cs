using System.Collections.Generic;
using UnityEngine;
using Battle.BattleEvents;
using Battle.UIEvents;
using UnityEngine.UI;

public class UIBattleManager : MonoBehaviour
{
    [Header("Battle Timeline")] [SerializeField]
    private UIBattleEntityTimeline battleEntityTimelinePrefab;

    [SerializeField] private Transform battleTimelineParent;

    [Header("Player Controls")] [SerializeField]
    private Button endTurnButton;

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

        EnablePlayerButtons(false);
    }


    private void ShowPlayerButtonsOnPlayerTurnStart(StartPlayerControlEvent ctx)
    {
        EnablePlayerButtons(true);
    }

    private void HidePlayerButtonsOnPlayerTurnEnd(EndPlayerControlEvent ctx)
    {
        EnablePlayerButtons(false);
    }
    
    private void EnablePlayerButtons(bool value)
    {
        endTurnButton.interactable = value;
    }

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
}