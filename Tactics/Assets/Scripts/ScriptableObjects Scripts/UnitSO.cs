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
        public float Initiative { get; private set; } = 1000;
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        [field: SerializeField] public List<AbilityToAdd> Abilities { get; private set; }
        
        [field: SerializeField] public List<PassiveToAdd> StartingPassives { get; private set; }
        public UnitStatsInstance CreateInstance(Unit unit) => new UnitStatsInstance(this,unit);
    }
}

namespace Battle
{
    using ScriptableObjects;

    [Serializable]
    public class AbilityToAdd
    {
        [field: SerializeField] public UnitAbilitySO Ability { get; private set; }
        [field: SerializeField] public bool ShowInUI { get; private set; } = true;

        public AbilityToAdd(UnitAbilitySO ability,bool showInUI = true)
        {
            Ability = ability;
            ShowInUI = showInUI;
        }
        
        public UnitAbilityInstance CreateInstance()
        {
            return new UnitAbilityInstance(this);
        }
    }
    
    [Serializable]
    public class PassiveToAdd
    {
        [field: SerializeField] public PassiveSO<Unit> UnitPassive { get; private set; }
        [field: SerializeField] public int UnitStacks { get; private set; } = 1;
        [field: SerializeField] public PassiveSO<Tile> TilePassive { get; private set; }
        [field: SerializeField] public int TileStacks { get; private set; } = 1;
        [field: SerializeField] public bool TargetTileFirst { get; private set; } = false;
        [field: SerializeField] public bool IgnoreTileIfUnit { get; private set; } = false;
        [field: SerializeField] public bool IgnoreUnitIfTile { get; private set; } = false;

        public bool IsType(PassiveType type)
        {
            var unitMatch = UnitPassive != null ? UnitPassive.Type == type : false;
            var tileMatch = TilePassive != null ? TilePassive.Type == type : false;
            
            return TargetTileFirst ? tileMatch : unitMatch;
        }
        
        public string GetText()
        {
            var unitText = UnitPassive != null ? $"<color=yellow>{(UnitPassive.IsStackable ? $" {UnitStacks} stack{(UnitStacks > 1 ? "s":"")} of ":"")}" + 
                                                 $" <u><link=\"passive:{0}\">{UnitPassive.Name}</link></u></color>" : string.Empty;
            var tileText =  TilePassive != null ? $"<color=yellow>{(TilePassive.IsStackable ? $" {TileStacks} stack{(TileStacks > 1 ? "s":"")} of ":"")}" + 
                                                  $" <u><link=\"passive:{0}\">{TilePassive.Name}</link></u></color>" : string.Empty;
            
            if(unitText == string.Empty && tileText == string.Empty) return string.Empty;
            if(unitText == string.Empty && tileText != string.Empty) return $"{tileText}";
            if(unitText != string.Empty && tileText == string.Empty) return $"{unitText}";

            if (TargetTileFirst) return $"{tileText} and {unitText}";
            return $"{unitText} and {tileText}";
        }
        

        public IEnumerator AddPassive(Tile tile)
        {
            if(tile == null) yield break;
            
            var tileRoutine = TilePassive != null ? tile.AddPassiveEffect(TilePassive,TileStacks) : null;
            var unitRoutine = UnitPassive != null
                ? tile.HasUnit() ? tile.Unit.AddPassiveEffect(UnitPassive, UnitStacks) : null
                : null;

            if (IgnoreUnitIfTile && tileRoutine != null) unitRoutine = null;
            if(IgnoreTileIfUnit && unitRoutine != null) tileRoutine = null;

            if(TargetTileFirst && tileRoutine != null) yield return tile.StartCoroutine(tileRoutine);
            if (unitRoutine != null) yield return tile.StartCoroutine(unitRoutine);
            if(!TargetTileFirst && tileRoutine != null) yield return tile.StartCoroutine(tileRoutine);
        }
    }
    
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
        
        //Shield
        public int BaseMaxShield => So.MaxShield;
        public int MaxShieldModifier { get; private set; }
        public int MaxShield => BaseMaxShield + MaxShieldModifier < 0 ? 0 : BaseMaxShield + MaxShieldModifier;
        public int MaxShieldDiff => MaxShieldModifier == 0 ? 0 : MaxShieldModifier > 0 ? 1 : -1;
        public event Action<UnitStatsInstance> OnMaxShieldModified;
        public void IncreaseMaxShieldModifier(int amount)
        {
            MaxShieldModifier += amount;
            OnMaxShieldModified?.Invoke(this);
        }
        
        private int currentShield;
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
            CurrentShield = MaxShield;
            
            Behaviour.InitBehaviour(unit);
        }

        public void ResetModifiers()
        {
            MaxHpModifier = 0;
            MaxShieldModifier = 0;
            AttackModifier = 0;
            AttackRangeModifier = 0;
            MovementModifier = 0;
            SpeedModifier = 0;
            
            OnMaxHpModified?.Invoke(this);
            OnMaxShieldModified?.Invoke(this);
            OnAttackModified?.Invoke(this);
            OnAttackRangeModified?.Invoke(this);
            OnMovementModified?.Invoke(this);
            OnSpeedModified?.Invoke(this);
        }
    }
}