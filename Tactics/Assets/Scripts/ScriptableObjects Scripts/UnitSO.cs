using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using Ability;
    
    [CreateAssetMenu(menuName = "Unit")]
    public class UnitSO : ScriptableObject
    {
        [field:Header("Visual")]
        [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public BattleModel model;
        
        [field:Header("Stats")]
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public int Attack { get; private set; } = 1; // TODO - probably change name
        [field: SerializeField] public int AttackRange { get; private set; } = 3;
        [field: SerializeField, Tooltip("Maximum Tiles that can be moved during a turn")]
        public int BaseMovement { get; private set; } = 3;
        
        [field: SerializeField, Tooltip("Turn Value Decay Rate, higher is faster")]
        public int BaseSpeed { get; private set; } = 100;
        [field: SerializeField, Tooltip("Start Turn Value, lower is faster")]
        public float Initiative { get; private set; } = 1000;
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        [field: SerializeField] public List<UnitAbilitySO> Abilities { get; private set; }
        public UnitStatsInstance CreateInstance(Unit unit) => new UnitStatsInstance(this,unit);
    }
}

namespace Battle
{
    using ScriptableObjects;
    
    public class UnitStatsInstance
    {
        public UnitSO So { get; }
        
        // Hp
        public int BaseMaxHp => So.MaxHp;
        public int MaxHpModifier { get; private set; }
        public int MaxHp => BaseMaxHp + MaxHpModifier < 0 ? 0 : BaseMaxHp + MaxHpModifier;
        public int MaxHpDiff => MaxHpModifier == 0 ? 0 : MaxHpModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMaxHpModified;
        public void IncreaseMaxHpModifier(int amount)
        {
            MaxHpModifier += amount;
            OnMaxHpModified?.Invoke(this);
        }
        
        private int currentHp;
        public event Action<UnitStatsInstance> OnCurrentHpModified;
        public int CurrentHp
        {
            get
            {
                if (currentHp > MaxHp) currentHp = MaxHp;
                return currentHp;
            }
            set
            {
                currentHp = value;
                OnCurrentHpModified?.Invoke(this);
            } 
        }

        // Attack
        public int BaseAttack => So.Attack;
        public int AttackModifier { get; private set; }
        public int Attack => BaseAttack + AttackModifier < 0 ? 0 : BaseAttack + AttackModifier;
        public int AttackDiff => AttackModifier == 0 ? 0 : AttackModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnAttackModified;
        public void IncreaseAttackModifier(int amount)
        {
            AttackModifier += amount;
            OnAttackModified?.Invoke(this);
        }
        
        // Attack Range
        public int BaseAttackRange => So.AttackRange;
        public int AttackRangeModifier { get; private set; }
        public int AttackRange => BaseAttackRange + AttackRangeModifier < 0 ? 0 : BaseAttackRange + AttackRangeModifier;
        public int AttackRangeDiff => AttackRangeModifier == 0 ? 0 : AttackRangeModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnAttackRangeModified;
        public void IncreaseAttackRangeModifier(int amount)
        {
            AttackRangeModifier += amount;
            OnAttackRangeModified?.Invoke(this);
        }
        
        // Movement
        public int BaseMovement => So.BaseMovement;
        public int MovementModifier { get; private set; }
        public int Movement => BaseMovement + MovementModifier < 0 ? 0 : BaseMovement + MovementModifier;
        public int MovementDiff => MovementModifier == 0 ? 0 : MovementModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMovementModified;
        public void IncreaseMovementModifier(int amount)
        {
            MovementModifier += amount;
            OnMovementModified?.Invoke(this);
        }
        
        // Turn Order
        public int BaseSpeed { get; }
        public int SpeedModifier { get; private set; }
        public int Speed => BaseSpeed + SpeedModifier < 0 ? 0 : BaseSpeed + SpeedModifier;
        public int SpeedDiff => SpeedModifier == 0 ? 0 : SpeedModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnSpeedModified;
        public void IncreaseSpeedModifier(int amount)
        {
            SpeedModifier += amount;
            OnSpeedModified?.Invoke(this);
        }
        public float Initiative { get; }
        
        // Behaviour
        public UnitBehaviourSO Behaviour { get; }
        
        public UnitStatsInstance(UnitSO so,Unit unit)
        {
            So = so;
            
            BaseSpeed = So.BaseSpeed;
            Initiative = So.Initiative;
            Behaviour = So.Behaviour;
            
            ResetModifiers();
            
            CurrentHp = MaxHp;
            
            Behaviour.InitBehaviour(unit);
        }

        public void ResetModifiers()
        {
            MaxHpModifier = 0;
            AttackModifier = 0;
            AttackRangeModifier = 0;
            MovementModifier = 0;
            SpeedModifier = 0;
            
            OnMaxHpModified?.Invoke(this);
            OnAttackModified?.Invoke(this);
            OnAttackRangeModified?.Invoke(this);
            OnMovementModified?.Invoke(this);
            OnSpeedModified?.Invoke(this);
        }
    }
}