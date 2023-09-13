using System;
using Battle;
using UnityEngine;

public abstract class UnitAbilitySO : ScriptableObject
{
    public Func<Tile, bool> TileSelector { get; protected set; } = tile => tile != null;
    
    [field:SerializeField] public int ExpectedSelectionAmount { get; private set; }

    public abstract void CastAbility(Unit[] targetUnits,Tile[] targetTiles);
}
