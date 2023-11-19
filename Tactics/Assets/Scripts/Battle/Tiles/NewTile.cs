using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    using ActionSystem;
    
    public class NewTile : IPassivesContainer
    {
        public Tile Tile { get; private set; }
        public NewUnit Unit { get; private set; }
        public Vector2Int Position { get; }
        
        public bool IsWalkable { get; private set; }
        private int CostToMove => 1; // If we need weights its here lmao (not used btw)
        private Tile[] neighbors; //0 is top (x,y+1), then clockwise, adjacent before diag
        
        public enum Direction
        {
            Top=0,
            Right=1,
            Down=2,
            Left=3,
            TopRight=4,
            DownRight=5,
            DownLeft=6,
            TopLeft=7
        }
        
        private List<PassiveInstance> passiveInstances { get; }

        public NewTile(Vector2Int position,Tile tile)
        {
            Position = position;
            Tile = tile;
            Tile.SetNewTile(this);

            passiveInstances = new List<PassiveInstance>();
            
            EventManager.AddListener<AddPassiveBattleAction>(AddPassiveInstanceToList);
        }

        private void AddPassiveInstanceToList(AddPassiveBattleAction ctx)
        {
            if(ctx.Container != this || ctx.PassiveInstance != null) return;
            
            passiveInstances.Add(ctx.PassiveInstance);
        }
        
        public PassiveInstance GetPassiveInstance(PassiveSO passiveSo)
        {
            return passiveInstances.FirstOrDefault(passiveInstance => passiveInstance.SO == passiveSo);
        }

        public void AddPassiveEffect(PassiveSO passiveSo, int amount = 1)
        {
            BattleAction.StartNewBattleAction(new AddPassiveBattleAction(this,passiveSo,amount));
        }

        public void RemovePassive(PassiveSO passiveSo)
        {
            var currentInstance = GetPassiveInstance(passiveSo);
            RemovePassiveInstance(currentInstance);
        }

        public void RemovePassiveInstance(PassiveInstance passiveInstance)
        {
            if(passiveInstance == null) return;
            if(!passiveInstances.Contains(passiveInstance)) return;
            
            passiveInstances.Remove(passiveInstance);
            BattleAction.StartNewBattleAction(new RemovePassiveBattleAction(this,passiveInstance));
        }

        public int GetPassiveEffectCount(Func<PassiveInstance, bool> condition, out PassiveInstance firstPassiveInstance)
        {
            firstPassiveInstance = passiveInstances.FirstOrDefault(condition);
            
            return passiveInstances.Count(condition);
        }
    }

}
