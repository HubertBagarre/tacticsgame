using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Unit")]
    public class UnitStatsSO : ScriptableObject
    {
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        
        [field: SerializeField] public List<UnitAbilitySO> Abilities { get; private set; }

        [field: SerializeField, Tooltip("Maximum Tiles that can be moved during a turn")]
        public int BaseMovement { get; private set; } = 3;

        [field: SerializeField, Tooltip("Start Turn Value, lower is faster")]
        public float Initiative { get; private set; } = 1000;

        [field: SerializeField, Tooltip("Turn Value Decay Rate, higher is faster")]
        public int BaseSpeed { get; private set; } = 100;

        // See UnitAbilitySO if can't be overriden (use a virtual bool func(Unit,Tile) instead)
        public Func<Tile, bool> WalkableTileSelector {  get; protected set; } =
            tile => tile.IsWalkable && !tile.HasUnit();
    }
}