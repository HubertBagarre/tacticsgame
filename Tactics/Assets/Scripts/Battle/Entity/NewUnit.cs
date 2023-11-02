using System.Collections;
using System.Collections.Generic;
using Battle.ScriptableObjects;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    
    public class NewUnit : TimelineEntity
    {
        public UnitStatsInstance Stats { get; private set; }
        public Tile Tile { get; private set; }
        
        
        public NewUnit(UnitSO so,Tile tile) : base(so.BaseSpeed, so.Initiative, so.Name)
        {
            Stats = so.CreateInstance();
            Tile = tile;
        }

        protected override void AddedToTimelineEffect()
        {
            
        }

        protected override void TurnStart()
        {
            Debug.Log("Here");
        }

        protected override void TurnEnd()
        {
        }
    }
}


