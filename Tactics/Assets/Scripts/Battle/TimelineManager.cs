using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.ScriptableObjects;
using UnityEngine;

namespace Battle
{
    public class TimelineManager : MonoBehaviour
    {
        [Serializable]
        public struct TimelineEntityShower
        {
            private TimelineEntity linkedEntity;
            
            [field: SerializeField] public string Name { get; private set; }
            [field: SerializeField] public string Team { get; private set; }
            [field: SerializeField] public string Initiative { get; private set; }
            [field: SerializeField] public string Speed { get; private set; }
            [field: SerializeField] public string DistanceFromStart { get; private set; }
            [field: SerializeField] public string TurnOrder { get; private set; }
            [field: SerializeField] public string JoinedIndex { get; private set; }
            [field: SerializeField] public string DecayRate { get; private set; }

            public TimelineEntityShower(TimelineEntity entity)
            {
                linkedEntity = entity;
                Name = linkedEntity.Name;
                Team = $"{linkedEntity.Team}";
                Initiative = $"{linkedEntity.Initiative}";
                Speed = $"{linkedEntity.Speed}";
                DistanceFromStart = $"{linkedEntity.DistanceFromTurnStart}";
                TurnOrder = $"{linkedEntity.TurnOrder}";
                JoinedIndex = $"{linkedEntity.JoinedIndex}";
                DecayRate = $"{linkedEntity.DecayRate}";
            }

            public void UpdateValues()
            {
                Name = linkedEntity.Name;
                Team = $"{linkedEntity.Team}";
                Initiative = $"{linkedEntity.Initiative}";
                Speed = $"{linkedEntity.Speed}";
                DistanceFromStart = $"{linkedEntity.DistanceFromTurnStart}";
                TurnOrder = $"{linkedEntity.TurnOrder}";
                JoinedIndex = $"{linkedEntity.JoinedIndex}";
                DecayRate = $"{linkedEntity.DecayRate}";
            }
        }
        
        private List<TimelineEntity> entitiesInTimeline = new();

        //Timeline
        [field: Header("Timeline")]
        [field: SerializeField]
        public int ResetTurnValue { get; private set; } = 100;
        
        [SerializeField]
        private List<TimelineEntityShower> timelineEntityShowers = new();
        
        public TimelineEntity FirstTimelineEntity => EntitiesInTimeline.FirstOrDefault();
        public bool IsFirstEntityRoundEnd => FirstTimelineEntity == roundEndEntity;
        private int totalEntityAddedToTimeline = 0;

        private RoundEndEntity roundEndEntity;

        public IList<TimelineEntity> EntitiesInTimeline => entitiesInTimeline.AsReadOnly();

        public void CreateNewTimeline()
        {
            entitiesInTimeline = new List<TimelineEntity>();
            
            roundEndEntity = new RoundEndEntity(0, 0, "End Round");
            
            AddEntityToTimeline(roundEndEntity,false);
            
        }
        
        public void AddEntitiesToTimeline(bool useInitiative,List<TimelineEntity> entities)
        {
            foreach (var entityToAdd in entities)
            {
                AddEntityToTimeline(entityToAdd, useInitiative);
            }
        }

        public void AddEntityToTimeline(TimelineEntity entityToAdd,bool useInitiative)
        {
            InsertEntityInTimeline(entityToAdd, useInitiative ? entityToAdd.Initiative : ResetTurnValue);
        }
        
        public void AddUnitToTimeline(NewUnit unit,bool useInitiative)
        {
            AddEntityToTimeline(unit, useInitiative);
        }

        public void InsertEntityInTimeline(TimelineEntity entityToInsert,float distanceFromTurnStart)
        {
            Debug.Log($"Inserting {entityToInsert.Name} at {distanceFromTurnStart}");
            
            entityToInsert.SetJoinIndex(totalEntityAddedToTimeline);
            totalEntityAddedToTimeline++;
            if (entityToInsert == roundEndEntity)
            {
                entityToInsert.SetJoinIndex(-1);
                totalEntityAddedToTimeline--;
            }
            
            entityToInsert.SetDistanceFromTurnStart(distanceFromTurnStart);
            
            entitiesInTimeline.Add(entityToInsert);
            
            timelineEntityShowers.Add(new TimelineEntityShower(entityToInsert));
            
            ReorderTimeline();
            
            entityToInsert.OnAddedToTimeline();

            UpdateShowers();
        }

        [ContextMenu("Reorder Timeline")]
        public void ReorderTimeline()
        {
            entitiesInTimeline = entitiesInTimeline.OrderBy(entity => entity).ToList();

            UpdateShowers();
            
            EventManager.Trigger(entitiesInTimeline);
        }

        public void ResetTimelineEntityDistanceFromTurnStart(TimelineEntity timelineEntity)
        {
            if (timelineEntity == roundEndEntity)
            {
                ResetRoundEntityDistanceFromTurnStart();
                return;
            }
            SetTimelineEntityDistanceFromTurnStart(timelineEntity,ResetTurnValue);
        }
        
        public void ResetRoundEntityDistanceFromTurnStart()
        {
            var distance = roundEndEntity.DistanceFromTurnStart;
            var speed = roundEndEntity.Speed;
            if (entitiesInTimeline.Count > 1)
            {
                ReorderTimeline();
                
                var slowestEntity = entitiesInTimeline.Where(entity => entity != roundEndEntity)
                    .OrderBy(entity => entity.Speed).First();
                speed = slowestEntity.Speed;
                
                var furthestEntity = entitiesInTimeline.Where(entity => entity != roundEndEntity)
                    .OrderBy(entity => entity.TurnOrder).Last();
                distance = furthestEntity.DistanceFromTurnStart; // + 0.01f; // TODO : find a better way to do this (+0.01f should not be necessary)
                
            }
            
            roundEndEntity.SetSpeed(speed);
            roundEndEntity.SetDistanceFromTurnStart(distance);
        }

        public void SetTimelineEntityDistanceFromTurnStart(TimelineEntity timelineEntity,float value)
        {
            timelineEntity?.SetDistanceFromTurnStart(value);
        }
        
        public void DecayEntitiesTurnValues(TimelineEntity timelineEntity,float amount)
        {
            timelineEntity.DecayTurnValue(amount);
        }

        [ContextMenu("Advance Timeline")]
        public void AdvanceTimeline()
        {
            var first = FirstTimelineEntity;
            
            ResetTimelineEntityDistanceFromTurnStart(first);
            
            ReorderTimeline();
            
            var amountToDecay = FirstTimelineEntity.TurnOrder;
            
            foreach (var entity in entitiesInTimeline)
            {
                DecayEntitiesTurnValues(entity, amountToDecay);
            }
        }

        private void UpdateShowers()
        {
            timelineEntityShowers.Clear();
            foreach (var timelineEntity in entitiesInTimeline)
            {
                var shower = new TimelineEntityShower(timelineEntity);
                timelineEntityShowers.Add(shower);
                shower.UpdateValues();
            }
        }

        private class RoundEndEntity : TimelineEntity
        {
            public override IEnumerable<StackableAction.YieldedAction> EntityTurnYieldedActions { get; }
        
            public RoundEndEntity(int speed, int initiative, string name) : base(speed, initiative, name)
            {
            }

            protected override void AddedToTimelineEffect()
            {
            
            }

            protected override void TurnStart()
            {
            }

            protected override void TurnEnd()
            {
            }

        }
    }
}


