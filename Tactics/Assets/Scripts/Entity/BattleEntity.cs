using UnityEngine;

public interface BattleEntity
{
    public Sprite Portrait { get; }
    
    public int Speed { get; }
    public float DistanceFromTurnStart { get; }

    public float TurnOrder => DistanceFromTurnStart / (Speed/100f);
    
    public void ResetTurnValue(float value);
    public void DecayTurnValue(float amount);
    public void StartTurn();
    public void EndTurn();
}