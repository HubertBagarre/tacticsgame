using System;
using System.Collections;
using Battle;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class UnitBehaviourSO : ScriptableObject
    {
        // TODO - Remove if useless
        protected static BattleManager battleM;
        protected static TileManager tileM;
        protected static UnitManager unitM;
        protected static AbilityManager abilityM;
        
        // See UnitAbilitySO if can't be overriden (use a virtual bool func(Unit,Tile) instead)
        public virtual Func<Tile, bool> WalkableTileSelector {  get; protected set; } = tile => tile.IsWalkable && !tile.HasUnit();

        public static void SetBattleManager(BattleManager battleManager)
        {
            battleM = battleManager;
        }

        public static void SetTileManager(TileManager tileManager)
        {
            tileM = tileManager;
        }

        public static void SetUnitManager(UnitManager unitManager)
        {
            unitM = unitManager;
        }
        
        public static void SetAbilityManager(AbilityManager abilityManager)
        {
            abilityM = abilityManager;
        }

        public abstract void InitBehaviour(Unit unit);
        public abstract void ShowBehaviourPreview(Unit unit); // when you hover on timeline
    
        public abstract IEnumerator RunBehaviour(Unit unit);

        public abstract bool OnBehaviourInterrupted(Unit unit); // returns true if interrupt behaviour this frame
    }
}
