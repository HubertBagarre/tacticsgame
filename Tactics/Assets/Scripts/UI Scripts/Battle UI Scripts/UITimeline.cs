using System.Collections.Generic;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UITimeline : MonoBehaviour
    {
        [SerializeField] private UIBattleEntityTimeline entityTimelinePrefab;
        [SerializeField] private Transform timelineContainer;
        private Dictionary<TimelineEntity, UIBattleEntityTimeline> uiEntityTimelines;
        
        private void Start()
        {
            uiEntityTimelines = new Dictionary<TimelineEntity, UIBattleEntityTimeline>();
            
            EventManager.AddListener<List<TimelineEntity>>(UpdateTimeline);
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
            
            uiEntityTimelines.Add(timelineEntity, uiEntity);

            return uiEntity;
        }

        
    }
}

