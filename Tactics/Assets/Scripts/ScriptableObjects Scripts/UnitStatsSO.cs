using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit")]
public class UnitStatsSO : ScriptableObject
{
    [field: SerializeField,Tooltip("Maximum Tiles that can be moved during a turn")] public int BaseMovement { get; private set; } = 3;
    [field: SerializeField,Tooltip("Start Turn Value, lower is faster")] public float Initiative { get; private set; } = 1000;
    [field: SerializeField,Tooltip("Turn Value Decay Rate, higher is faster")] public int BaseSpeed { get; private set; } = 100;

    public Func<Tile, bool> WalkableTileSelector { get; protected set; } = tile => tile.IsWalkable && !tile.HasUnit();
}
