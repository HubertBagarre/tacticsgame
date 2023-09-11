using System.Collections.Generic;
using UnityEngine;
using Battle.BattleEvents;

public class UIBattleManager : MonoBehaviour
{
    [Header("Battle Timeline")]
    [SerializeField] private UIBattleEntityTimeline battleEntityTimelinePrefab;
    [SerializeField] private Transform battleTimelineParent;

    private Dictionary<BattleEntity, UIBattleEntityTimeline> uibattleTimelineDict = new ();

    private void Start()
    {
        EventManager.AddListener<EntityJoinBattleEvent>(InstantiateBattleEntityTimelineUI);
        EventManager.AddListener<EntityLeaveBattleEvent>(RemoveBattleEntityTimelineUI);
        EventManager.AddListener<UpdateTurnValuesEvent>(ReorderBattleEntityTimeline);
    }

    private void InstantiateBattleEntityTimelineUI(EntityJoinBattleEvent ctx)
    {
        var entity = ctx.Entity;

        var ui = Instantiate(battleEntityTimelinePrefab, battleTimelineParent);

        uibattleTimelineDict.Add(entity,ui);
        
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

            //ui.Show(true);
            ui.Show(i <= roundIndex);
        }
    }
}
