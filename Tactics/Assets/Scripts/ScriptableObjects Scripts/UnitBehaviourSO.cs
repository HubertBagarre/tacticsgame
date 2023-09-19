using System.Collections;
using Battle;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class UnitBehaviourSO : ScriptableObject
    {
        protected static BattleManager battleM;
        protected static TileManager tileM;
        protected static UnitManager unitM;
        protected static AbilityManager abilityM;

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

        public abstract void InterruptBehaviour(Unit unit);
    }
}
