using Battle;
using UnityEngine;

public abstract class UnitBehaviourSO : ScriptableObject
{
    protected static BattleManager battleM;
    protected static TileManager tileM;
    protected static UnitManager unitM;

    public static void SetTurnManager(BattleManager battleManager)
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

    public abstract void InitBehaviour(Unit unit);
    public abstract void ShowBehaviourPreview(Unit unit); // when you hover on timeline
    
    public abstract void RunBehaviour(Unit unit);
}
