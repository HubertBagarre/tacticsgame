using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
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
        [field: SerializeField] public int MaxShield { get; private set; } = 3;
        [field: SerializeField] public int Attack { get; private set; } = 1; // TODO - probably change name
        [field: SerializeField] public int AttackRange { get; private set; } = 3;
        [field: SerializeField, Tooltip("Maximum Tiles that can be moved during a turn")]
        public int BaseMovement { get; private set; } = 3;
        
        [field: SerializeField, Tooltip("Turn Value Decay Rate, higher is faster")]
        public int BaseSpeed { get; private set; } = 100;
        [field: SerializeField, Tooltip("Start Turn Value, lower is faster")]
        public int Initiative { get; private set; } = 1000;
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        [field: SerializeField] public List<AbilityToAdd> Abilities { get; private set; }
        [field: SerializeField] public List<PassiveToAdd> StartingPassives { get; private set; }
        public UnitStatsInstance CreateInstance(Unit unit) => new UnitStatsInstance(this,unit);
        public UnitStatsInstance CreateInstance() => new UnitStatsInstance(this);
    }
}

namespace Battle
{
    using ScriptableObjects;

    [Serializable]
    public struct AbilityToAdd
    {
        [field: SerializeField] public UnitAbilitySO Ability { get; private set; }
        [field: SerializeField] public NewAbilitySO NewAbility { get; private set; }
        [field: SerializeField] public bool ShowInUI { get; private set; }

        public AbilityToAdd(NewAbilitySO ability,bool showInUI = true)
        {
            Ability = null;
            NewAbility = ability;
            ShowInUI = showInUI;
        }

        public AbilityInstance CreateInstance()
        {
            return new AbilityInstance(this);
        }
    }
    
    [Serializable]
    public class PassiveToAdd
    {
        [field: SerializeField] public PassiveSO SO { get; private set; }
        [field: SerializeField] public int Stacks { get; private set; } = 1;
        
        public string GetText()
        {
            var text = SO != null ? $"<color=yellow>{(SO.IsStackable ? $" {Stacks} stack{(Stacks > 1 ? "s":"")} of ":"")}" + 
                                                 $"<u><link=\"passive:{0}\">{SO.Name}</link></u></color>" : string.Empty;
            /*var tileText =  TilePassive != null ? $"<color=yellow>{(TilePassive.IsStackable ? $" {TileStacks} stack{(TileStacks > 1 ? "s":"")} of ":"")}" + 
                                                  $"<u><link=\"passive:{0}\">{TilePassive.Name}</link></u></color>" : string.Empty;
            
            if(unitText == string.Empty && tileText == string.Empty) return string.Empty;
            if(unitText == string.Empty && tileText != string.Empty) return $"{tileText}";
            if(unitText != string.Empty && tileText == string.Empty) return $"{unitText}";

            if (TargetTileFirst) return $"{tileText} and {unitText}";*/
            return $"{text}";
        }
        

        public void AddPassiveToContainer(IPassivesContainer container)
        {
            
            
            
            container?.AddPassiveEffect(SO,Stacks);
            
            /*
            var tileRoutine = TilePassive != null ? tile.AddPassiveEffect(TilePassive,TileStacks) : null;
            var unitRoutine = SO != null
                ? tile.HasUnit() ? tile.Unit.AddPassiveEffect(SO, Stacks) : null
                : null;

            if (IgnoreUnitIfTile && tileRoutine != null) unitRoutine = null;
            if(IgnoreTileIfUnit && unitRoutine != null) tileRoutine = null;

            if(TargetTileFirst && tileRoutine != null) yield return tile.StartCoroutine(tileRoutine);
            if (unitRoutine != null) yield return tile.StartCoroutine(unitRoutine);
            if(!TargetTileFirst && tileRoutine != null) yield return tile.StartCoroutine(tileRoutine);*/
        }
    }
    
    [Serializable]
    public class UnitStatsInstance
    {
        public UnitSO So { get; }
        
        // Hp
        public int BaseMaxHp => So.MaxHp;
        [field: SerializeField] public int MaxHpModifier { get; private set; }
        public int MaxHp => BaseMaxHp + MaxHpModifier < 0 ? 0 : BaseMaxHp + MaxHpModifier;
        public int MaxHpDiff => MaxHpModifier == 0 ? 0 : MaxHpModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMaxHpModified;
        public void IncreaseMaxHpModifier(int amount)
        {
            MaxHpModifier += amount;
            OnMaxHpModified?.Invoke(this);
        }
        
        [field: SerializeField] private int currentHp;
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
        
        //Shield
        public int BaseMaxShield => So.MaxShield;
        [field: SerializeField] public int MaxShieldModifier { get; private set; }
        public int MaxShield => BaseMaxShield + MaxShieldModifier < 0 ? 0 : BaseMaxShield + MaxShieldModifier;
        public int MaxShieldDiff => MaxShieldModifier == 0 ? 0 : MaxShieldModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMaxShieldModified;
        public void IncreaseMaxShieldModifier(int amount)
        {
            MaxShieldModifier += amount;
            OnMaxShieldModified?.Invoke(this);
        }
        
        [field: SerializeField] private int currentShield;
        public event Action<UnitStatsInstance> OnCurrentShieldModified;
        public int CurrentShield
        {
            get
            {
                if (currentShield > MaxShield) currentShield = MaxShield;
                return currentShield;
            }
            set
            {
                currentShield = value;
                if(currentShield < 0) currentShield = 0;
                OnCurrentShieldModified?.Invoke(this);
            } 
        }
        
        // Break
        [field: SerializeField] private bool isBroken;
        public event Action<UnitStatsInstance> OnBreakValueChanged; 
        public bool IsBroken
        {
            get => isBroken;
            private set
            {
                var changed = isBroken != value;
                isBroken = value;
                if(changed) OnBreakValueChanged?.Invoke(this);
            } 
        }
        

        // Attack
        public int BaseAttack => So.Attack;
        [field: SerializeField] public int AttackModifier { get; private set; }
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
        [field: SerializeField] public int AttackRangeModifier { get; private set; }
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
        [field: SerializeField] public int MovementModifier { get; private set; }
        public int Movement => BaseMovement + MovementModifier < 0 ? 0 : BaseMovement + MovementModifier;
        public int MovementDiff => MovementModifier == 0 ? 0 : MovementModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMovementModified;
        public void IncreaseMovementModifier(int amount)
        {
            MovementModifier += amount;
            OnMovementModified?.Invoke(this);
        }
        
        [field: SerializeField] private int currentMovement;
        public event Action<UnitStatsInstance> OnCurrentMovementModified;
        public int CurrentMovement
        {
            get => currentMovement;
            set
            {
                currentMovement = value;
                OnCurrentMovementModified?.Invoke(this);
            } 
        }
        
        // Turn Order
        public int BaseSpeed { get; }
        [field: SerializeField] public int SpeedModifier { get; private set; }
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
            CurrentShield = MaxShield;
            
            Behaviour.InitBehaviour(unit);
        }
        
        public UnitStatsInstance(UnitSO so)
        {
            So = so;
            
            BaseSpeed = So.BaseSpeed;
            Initiative = So.Initiative;
            Behaviour = So.Behaviour;
            
            ResetModifiers();
            
            CurrentHp = MaxHp;
            CurrentShield = MaxShield;

            IsBroken = false;

            //Behaviour.InitBehaviour(unit);
        }

        public void ResetModifiers()
        {
            MaxHpModifier = 0;
            MaxShieldModifier = 0;
            AttackModifier = 0;
            AttackRangeModifier = 0;
            MovementModifier = 0;
            SpeedModifier = 0;
            
            RefreshModifiers();
        }

        public void RefreshModifiers()
        {
            OnMaxHpModified?.Invoke(this);
            OnMaxShieldModified?.Invoke(this);
            OnAttackModified?.Invoke(this);
            OnAttackRangeModified?.Invoke(this);
            OnMovementModified?.Invoke(this);
            OnSpeedModified?.Invoke(this);
        }

        public void ModifyStat(UnitStat stat, Operation operation,float value)
        {
            switch (stat)
            {
                case UnitStat.Hp:
                    CurrentHp = ModifyValue(CurrentHp);
                    // callback is already in CurrentHp setter
                    return;
                case UnitStat.MaxHp:
                    MaxHpModifier = ModifyValue(MaxHpModifier);
                    OnMaxHpModified?.Invoke(this);
                    return;
                case UnitStat.Movement:
                    MovementModifier = ModifyValue(MovementModifier);
                    OnMovementModified?.Invoke(this);
                    return;
                case UnitStat.CurrentMovement:
                    CurrentMovement = ModifyValue(CurrentMovement);
                    // callback is already in CurrentMovement setter
                    return;
                case UnitStat.Speed:
                    SpeedModifier = ModifyValue(SpeedModifier);
                    OnSpeedModified?.Invoke(this);
                    return;
                case UnitStat.Attack:
                    AttackModifier = ModifyValue(AttackModifier);
                    OnAttackModified?.Invoke(this);
                    return;
                case UnitStat.AttackRange:
                    AttackRangeModifier = ModifyValue(AttackRangeModifier);
                    OnAttackRangeModified?.Invoke(this);
                    return;
                case UnitStat.MaxShield:
                    MaxShieldModifier = ModifyValue(MaxShieldModifier);
                    OnMaxShieldModified?.Invoke(this);
                    return;
                case UnitStat.CurrentShield:
                    CurrentShield = ModifyValue(CurrentShield);
                    // callback is already in CurrentShield setter
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
            
            int ModifyValue(int statValue)
            {
                var result = operation switch
                {
                    Operation.Set => value,
                    Operation.Add => statValue + value,
                    Operation.Subtract => statValue - value,
                    Operation.Multiply => statValue * value,
                    Operation.Divide => statValue / value,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
                };

                return Mathf.FloorToInt(result);
            }
        }
        
        public static string UnitStatToText(UnitStat stat)
        {
            return stat switch
            {
                UnitStat.Hp => "HP",
                UnitStat.MaxHp => "Max HP",
                UnitStat.Movement => "Movement",
                UnitStat.CurrentMovement => "Movements Left",
                UnitStat.Speed => "Speed",
                UnitStat.Attack => "Attack",
                UnitStat.AttackRange => "Attack Range",
                UnitStat.MaxShield => "Max Shield",
                UnitStat.CurrentShield => "Shield",
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }

        public int GetStat(UnitStat stat)
        {
            return stat switch
            {
                UnitStat.Hp => CurrentHp,
                UnitStat.MaxHp => MaxHp,
                UnitStat.Movement => Movement,
                UnitStat.CurrentMovement => CurrentMovement,
                UnitStat.Speed => Speed,
                UnitStat.Attack => Attack,
                UnitStat.AttackRange => AttackRange,
                UnitStat.MaxShield => MaxShield,
                UnitStat.CurrentShield => CurrentShield,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
    }
}