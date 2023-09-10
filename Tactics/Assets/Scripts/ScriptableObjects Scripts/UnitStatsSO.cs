using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit")]
public class UnitStatsSO : ScriptableObject
{
    [field: SerializeField] public int movement;

    public Func<Tile, bool> WalkableTileSelector { get; protected set; } = tile => tile.Walkable && !tile.HasUnit();
}
