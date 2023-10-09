using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using Ability;
    
    [CreateAssetMenu(menuName = "Unit")]
    public class UnitStatsSO : ScriptableObject
    {
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public int BaseAttack { get; private set; } = 1; // TODO - probably change name
        [field: SerializeField, Tooltip("Turn Value Decay Rate, higher is faster")]
        public int BaseSpeed { get; private set; } = 100;
        [field: SerializeField, Tooltip("Start Turn Value, lower is faster")]
        public float Initiative { get; private set; } = 1000;
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        [field: SerializeField] public List<UnitAbilitySO> Abilities { get; private set; }

        [field: SerializeField, Tooltip("Maximum Tiles that can be moved during a turn")]
        public int BaseMovement { get; private set; } = 3;
    }
}