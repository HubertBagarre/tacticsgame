using System.Collections;
using Battle.ActionSystem.TimelineActions;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.UIComponent
{
    public class UITimeline : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform timelineContainer;
        [SerializeField] private GameObject timelineObj;
        
        [Header("Settings")]
        [SerializeField] private UIBattleEntityTimeline entityTimelinePrefab;
        private Dictionary<TimelineEntity, UIBattleEntityTimeline> uiEntityTimelines;
        private UIBattleEntityTimeline currentBattleEntityTimeline;
        
        private void Start()
        {
            uiEntityTimelines = new Dictionary<TimelineEntity, UIBattleEntityTimeline>();
            
            currentBattleEntityTimeline = Instantiate(entityTimelinePrefab, timelineContainer);
            
            ShowTimeline(false);
            
            AddCallbacks();
        }

        private void AddCallbacks()
        {
            TimelineManager.OnTimelineUpdated += UpdateTimeline;
            
            ActionStartInvoker<TimelineEntityTurnAction>.OnInvoked += ShowCurrentBattleEntityTimeline;
            ActionEndInvoker<TimelineEntityTurnAction>.OnInvoked += HideCurrentBattleEntityTimeline;

            ActionStartInvoker<NewBattleManager.RoundAction>.OnInvoked += HideTimelineDuringTransition;
        }

        private void RemoveCallbacks()
        {
            TimelineManager.OnTimelineUpdated -= UpdateTimeline;
            
            ActionStartInvoker<TimelineEntityTurnAction>.OnInvoked -= ShowCurrentBattleEntityTimeline;
            ActionEndInvoker<TimelineEntityTurnAction>.OnInvoked -= HideCurrentBattleEntityTimeline;
            
            ActionStartInvoker<NewBattleManager.RoundAction>.OnInvoked -= HideTimelineDuringTransition;
        }
        
        private void HideTimelineDuringTransition(NewBattleManager.RoundAction action)
        {
            ShowTimeline(false);

            StartCoroutine(DelayShowTimeline());
            
            return;
            IEnumerator DelayShowTimeline()
            {
                yield return new WaitForSeconds(action.TransitionDurationFloat);
                ShowTimeline();
            }
        }

        public void ShowTimeline(bool value = true)
        {
            // TODO - probably animation or idk
            
            timelineObj.SetActive(value);
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

        private void UpdateTimeline((List<TimelineEntity> timelineEntities,int endRoundEntityIndex) data)
        {
            for (var index = 0; index < data.timelineEntities.Count; index++)
            {
                var timelineEntity = data.timelineEntities[index];
                var uiEntity = GetUIBattleEntityTimeline(timelineEntity);
                uiEntity.transform.SetSiblingIndex(0);
                uiEntity.Show(index <= data.endRoundEntityIndex);
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

