using UnityEngine;

public interface BattleEntity
{
    public Sprite Portrait { get; }
    public int Speed { get; }
    public float DecayRate => Speed / 100f;
    public float TurnValue { get; }

    public void ResetTurnValue(float value);
    public void DecayTurnValue(float amount);

    public void StartTurn();
    public void EndTurn();
}
