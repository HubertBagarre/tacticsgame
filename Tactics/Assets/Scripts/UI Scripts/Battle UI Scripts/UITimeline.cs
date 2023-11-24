using Battle.ActionSystem.TimelineActions;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UITimeline : MonoBehaviour
    {
        [SerializeField] private UIBattleEntityTimeline entityTimelinePrefab;
        [SerializeField] private Transform timelineContainer;
        private Dictionary<TimelineEntity, UIBattleEntityTimeline> uiEntityTimelines;
        private UIBattleEntityTimeline currentBattleEntityTimeline;
        
        private void Start()
        {
            uiEntityTimelines = new Dictionary<TimelineEntity, UIBattleEntityTimeline>();
            
            currentBattleEntityTimeline = Instantiate(entityTimelinePrefab, timelineContainer);
            
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            EventManager.AddListener<List<TimelineEntity>>(UpdateTimeline);
            
            ActionStartInvoker<TimelineEntityTurnAction>.OnInvoked += ShowCurrentBattleEntityTimeline;
            ActionEndInvoker<TimelineEntityTurnAction>.OnInvoked += HideCurrentBattleEntityTimeline;
        }

        private void RemoveCallbacks()
        {
            EventManager.RemoveListener<List<TimelineEntity>>(UpdateTimeline);
            
            ActionStartInvoker<TimelineEntityTurnAction>.OnInvoked -= ShowCurrentBattleEntityTimeline;
            ActionEndInvoker<TimelineEntityTurnAction>.OnInvoked -= HideCurrentBattleEntityTimeline;
        }

        private void ShowCurrentBattleEntityTimeline(TimelineEntityTurnAction action)
        {
            currentBattleEntityTimeline.ConnectToEntity(action.Entity);
            currentBattleEntityTimeline.ChangeValue(-1);
            
            currentBattleEntityTimeline.Show(true);
            currentBattleEntityTimeline.ShowArrow(true);
        }
        
        private void HideCurrentBattleEntityTimeline(TimelineEntityTurnAction action)
        {
            currentBattleEntityTimeline.Show(false);
            currentBattleEntityTimeline.ShowArrow(false);
        }

        private void UpdateTimeline(List<TimelineEntity> timelineEntities)
        {
            foreach (var timelineEntity in timelineEntities)
            {
                var uiEntity = GetUIBattleEntityTimeline(timelineEntity);
                uiEntity.transform.SetSiblingIndex(0);
            }
        }

        private UIBattleEntityTimeline GetUIBattleEntityTimeline(TimelineEntity timelineEntity)
        {
            if (uiEntityTimelines.TryGetValue(timelineEntity, out var timeline)) return timeline;
            
            var uiEntity = Instantiate(entityTimelinePrefab, timelineContainer);
            uiEntity.ConnectToEntity(timelineEntity);
            uiEntity.ShowArrow(false);
            
            uiEntityTimelines.Add(timelineEntity, uiEntity);

            return uiEntity;
        }

        
    }
}

