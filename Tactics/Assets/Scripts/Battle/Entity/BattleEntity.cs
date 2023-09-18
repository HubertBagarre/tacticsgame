using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A BattleEntity is defined by having a place on the timeline, they include : Units (player controlled and ai), Unit Preview and Round End Entity
///
/// 
/// </summary>
public interface BattleEntity
{
    public Sprite Portrait { get; }
    public int Team { get; }
    
    public int Speed { get; }
    public float DistanceFromTurnStart { get; }
    public float TurnOrder => DistanceFromTurnStart / (Speed/100f);
    public bool IsDead { get; }
    public void InitEntityForBattle();
    public void KillEntityInBattle();
    public event Action OnDeath;
    public void ResetTurnValue(float value);
    public void DecayTurnValue(float amount);
    public IEnumerator StartRound();
    public IEnumerator EndRound();
    public IEnumerator StartTurn(Action onBehaviourEnd);
    public IEnumerator EndTurn();
}
