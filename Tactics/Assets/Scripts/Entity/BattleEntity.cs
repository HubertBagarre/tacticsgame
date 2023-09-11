public interface BattleEntity
{
    public int Speed { get; }
    public float DecayRate => Speed / 100f;
    public float TurnValue { get; }

    public void ResetTurnValue(bool useInitiative = false);
    public void DecayTurnValue(float amount);

    public void StartTurn();
    public void EndTurn();
}
