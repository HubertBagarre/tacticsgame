using UnityEngine;

/// <summary>
/// A BattleEntity is defined by having a place on the timeline, they include : Units (player controlled and ai), Unit Preview and Round End Entity
///
/// 
/// </summary>
public interface BattleEntity
{
    public Sprite Portrait { get; }
    
    public int Speed { get; }
    public float DistanceFromTurnStart { get; }

    public float TurnOrder => DistanceFromTurnStart / (Speed/100f);
    public void InitEntityForBattle();
    public void KillEntityInBattle();
    public void ResetTurnValue(float value);
    public void DecayTurnValue(float amount);
    public void StartTurn();
    public void EndTurn();
}
