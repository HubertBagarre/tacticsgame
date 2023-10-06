using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using ScriptableObjects;
    using ScriptableObjects.Ability;

    public class Unit : MonoBehaviour, BattleEntity
    {
        [field: SerializeField] public Tile Tile { get; private set; }
        [field: SerializeField] public int Team { get; private set; } //0 is player
        [field: SerializeField] public UnitStatsSO Stats { get; private set; }
        public Sprite Portrait => Stats.Portrait;

        [field: Header("Current Flags")]
        [field: SerializeField]
        public bool IsActive { get; private set; }

        [field: SerializeField] public bool CanMove { get; private set; } = true;

        [field: Header("Current Stats")]
        [field: SerializeField]
        public UnitBehaviourSO Behaviour { get; private set; }

        [field: SerializeField] public int Movement { get; private set; }
        public int Attack => Stats.BaseAttack + bonusAttack;
        [SerializeField] private int bonusAttack = 0;

        [field: SerializeField] public int MovementLeft { get; private set; }
        [field: SerializeField] public int Speed { get; protected set; }
        public float DecayRate => Speed / 100f;
        [field: SerializeField] public float DistanceFromTurnStart { get; protected set; }
        [field: SerializeField] public int CurrentHp { get; protected set; }
        public bool IsDead => CurrentHp <= 0;

        [field: SerializeField] public int CurrentUltimatePoints { get; protected set; }
        public int MaxUltimatePoints => GetHighestCostUltimate();
        public event Action<int, int> OnUltimatePointsAmountChanged;
        
        public List<UnitAbilityInstance> AbilityInstances { get; } = new();
        public List<UnitPassiveInstance> PassiveInstances { get; } = new();
        private List<UnitPassiveInstance> passivesToRemove = new();

        private Coroutine behaviourRoutine;

        private List<IEnumerator> onAttackOtherUnitRoutines = new ();
        private List<IEnumerator> onAttackedRoutines = new ();
        
        // TODO - use IEnumerator delegates instead of action;
        public event Action<int> OnCurrentHealthChanged;
        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action OnDeath;
        public event Action<UnitPassiveInstance> OnPassiveAdded; 
        public event Action<UnitPassiveInstance> OnPassiveRemoved; 

        public void InitUnit(Tile tile, int team, UnitStatsSO so)
        {
            Tile = tile;
            Team = team;
            Stats = so;

            Movement = so.BaseMovement;
            Speed = so.BaseSpeed;
            Behaviour = so.Behaviour;
            CurrentHp = so.MaxHp;

            CurrentUltimatePoints = 0;

            IsActive = true;

            AbilityInstances.Clear();
            behaviourRoutine = null;

            tile.SetUnit(this);
        }

        public void InitEntityForBattle()
        {
            Movement = Stats.BaseMovement;
            Speed = Stats.BaseSpeed;
            Behaviour = Stats.Behaviour;
            CurrentHp = Stats.MaxHp;

            bonusAttack = 0;

            CurrentUltimatePoints = 0;

            AbilityInstances.Clear();
            foreach (var ability in Stats.Abilities)
            {
                AbilityInstances.Add(ability.CreateInstance());
            }

            Behaviour.InitBehaviour(this);
        }

        public IEnumerator StartRound()
        {
            yield return null; //apply effects
        }

        public IEnumerator EndRound()
        {
            yield return null; //apply effects
        }

        public void InterruptBehaviour()
        {
            if (behaviourRoutine == null) return;

            Debug.Log("Interrupting behaviour");

            Behaviour.InterruptBehaviour(this);

            StopCoroutine(behaviourRoutine);
            behaviourRoutine = null;
        }

        public IEnumerator StartTurn(Action onBehaviourEnd)
        {
            MovementLeft = Movement;
            
            passivesToRemove.Clear();
            foreach (var passiveInstance in PassiveInstances)
            {
                if (passiveInstance.SO.HasStartTurnEffect) yield return StartCoroutine(passiveInstance.StartTurnEffect(this));
                if(passiveInstance.NeedRemoveOnTurnStart) passivesToRemove.Add(passiveInstance);
                if (IsDead)
                {
                    onBehaviourEnd.Invoke();
                    yield break;
                }
            }

            yield return StartCoroutine(RemovePassives());

            foreach (var abilityInstance in AbilityInstances)
            {
                abilityInstance.DecreaseCurrentCooldown(1);
            }

            //Do the remove passive on turn start here instead on in the loop
            OnTurnStart?.Invoke();

            EventManager.Trigger(new StartUnitTurnEvent(this));

            UIBattleManager.OnEndTurnButtonClicked += InterruptBehaviour;

            bool behaviourRunning = true;

            StartCoroutine(RunBehaviour());

            bool IsBehaviourRoutineRunning() => behaviourRoutine != null && behaviourRunning;

            yield return new WaitWhile(IsBehaviourRoutineRunning);

            UIBattleManager.OnEndTurnButtonClicked -= InterruptBehaviour;

            onBehaviourEnd.Invoke();

            IEnumerator RunBehaviour()
            {
                behaviourRoutine = StartCoroutine(Behaviour.RunBehaviour(this));
                yield return behaviourRoutine;
                behaviourRoutine = null;
                behaviourRunning = false;
            }
        }

        public IEnumerator EndTurn()
        {
            passivesToRemove.Clear();
            foreach (var passiveInstance in PassiveInstances.Where(unitPassiveInstance => unitPassiveInstance.SO.HasEndTurnEffect))
            {
                yield return StartCoroutine(passiveInstance.EndTurnEffect(this));
                if(passiveInstance.NeedRemoveOnTurnEnd) passivesToRemove.Add(passiveInstance);
            }
            
            yield return StartCoroutine(RemovePassives());

            OnTurnEnd?.Invoke();

            EventManager.Trigger(new EndUnitTurnEvent(this));
        }

        public void SetTile(Tile tile)
        {
            if (Tile != null) Tile.RemoveUnit();

            Tile = tile;

            Tile.SetUnit(this);
        }

        public IEnumerator MoveUnit(List<Tile> path,bool isForced) //PATH DOESN'T INCLUDE STARTING TILE
        {
            if (!path.Any())
            {
                yield break; // checks for valid path
            }

            if (path.Any(tile => tile.HasUnit()))
            {
                yield break; //does the path have any unit on it ?
            }

            if (!CanContinueMovement())
            {
                yield break;
            }

            for (var index = 0; index < path.Count && CanContinueMovement(); index++)
            {
                var tile = path[index];
                
                //TODO - play animation, different is isForced
                yield return new WaitForSeconds(0.1f);

                transform.position = tile.transform.position;

                if(!isForced) MovementLeft--;
                SetTile(tile);
            }

            bool CanContinueMovement()
            {
                return MovementLeft > 0 && !IsDead;
            }
        }

        public void ResetTurnValue(float value)
        {
            DistanceFromTurnStart = value < 0 ? Stats.Initiative : value;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public IEnumerator AttackUnitEffect(Unit attackedUnit,int damage)
        {
            var attackInstance = new AttackInstance(damage);
            
            foreach (var routine in onAttackOtherUnitRoutines)
            {
                yield return StartCoroutine(routine);
                if(IsDead) yield break;
            }
            
            yield return StartCoroutine(attackedUnit.AttackedUnitEffect(attackInstance));
        }

        public IEnumerator AttackedUnitEffect(AttackInstance attackInstance)
        {
            foreach (var routine in onAttackedRoutines)
            {
                yield return StartCoroutine(routine);
            }
            TakeDamage(attackInstance.Damage);

            yield return new WaitForSeconds(1f);
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) amount = 0; //No negative damage, negative damage doesn't heal, but can deal 0 damage

            var startHp = CurrentHp;
            CurrentHp -= amount;
            
            EventManager.Trigger(new UnitTakeDamageEvent(this, startHp));

            if (CurrentHp <= 0) KillEntityInBattle();
            OnCurrentHealthChanged?.Invoke(CurrentHp);
        }

        public void HealDamage(int amount)
        {
            if (amount < 0) amount = 0;

            var startHp = CurrentHp;
            CurrentHp += amount;

            EventManager.Trigger(new UnitHealDamageEvent(this, startHp));

            if (CurrentHp > Stats.MaxHp) CurrentHp = Stats.MaxHp; //No overheal (yet ?)
            OnCurrentHealthChanged?.Invoke(CurrentHp);
        }
        
        [ContextMenu("Execute")]
        public void Execute()
        {
            TakeDamage(CurrentHp);
        }

        public void KillEntityInBattle()
        {
            if(Tile != null) Tile.RemoveUnit();
            Speed = 0;

            OnDeath?.Invoke();
            OnDeath = null;

            EventManager.Trigger(new UnitDeathEvent(this));


            gameObject.SetActive(false);
        }

        private int GetHighestCostUltimate()
        {
            if (AbilityInstances.Count <= 0) return 0;
            return AbilityInstances.OrderByDescending(ability => ability.UltimateCost).First().UltimateCost;
        }

        public void GainUltimatePoint(int amount)
        {
            var previous = CurrentUltimatePoints;
            CurrentUltimatePoints += amount;
            if (CurrentUltimatePoints > MaxUltimatePoints) CurrentUltimatePoints = MaxUltimatePoints;

            OnUltimatePointsAmountChanged?.Invoke(previous, CurrentUltimatePoints);
        }

        public void ConsumeUltimatePoint(int amount)
        {
            var previous = CurrentUltimatePoints;
            CurrentUltimatePoints -= amount;
            if (CurrentUltimatePoints < 0) CurrentUltimatePoints = 0;

            OnUltimatePointsAmountChanged?.Invoke(previous, CurrentUltimatePoints);
        }

        public UnitPassiveInstance GetPassiveInstance(UnitPassiveSO passiveSo)
        {
            return PassiveInstances.FirstOrDefault(passiveInstance => passiveInstance.SO == passiveSo);
        }
        
        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveSo"></param>
        /// <returns></returns>
        public IEnumerator AddPassiveEffect(UnitPassiveSO passiveSo)
        {
            //add passive instance to list

            var currentInstance = GetPassiveInstance(passiveSo);

            //if current instance == null, no passive yet, creating new and adding to list
            //if passive isn't stackable, creating new and adding to list
            if (currentInstance == null || !passiveSo.IsStackable)
            {
                currentInstance = passiveSo.CreateInstance();
                PassiveInstances.Add(currentInstance);
            }
            
            OnPassiveAdded?.Invoke(currentInstance);

            return currentInstance.AddPassive(this);
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveSo"></param>
        /// <returns></returns>
        public IEnumerator RemovePassiveEffect(UnitPassiveSO passiveSo)
        {
            var currentInstance = GetPassiveInstance(passiveSo);
            return currentInstance == null ? null : RemovePassiveEffect(currentInstance);
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveInstance"></param>
        /// <returns></returns>
        public IEnumerator RemovePassiveEffect(UnitPassiveInstance passiveInstance)
        {
            if (!PassiveInstances.Contains(passiveInstance)) return null;
            PassiveInstances.Remove(passiveInstance);
            
            OnPassiveRemoved?.Invoke(passiveInstance);
            
            return passiveInstance.RemovePassive(this);
        }

        private IEnumerator RemovePassives()
        {
            foreach (var passiveToRemove in passivesToRemove)
            {
                yield return StartCoroutine(RemovePassiveEffect(passiveToRemove)); 
            }
            passivesToRemove.Clear();
        }
    }
    
    public class AttackInstance
    {
        public int OriginalDamage { get; }
        public int Damage { get; private set; }

        public AttackInstance(int damage)
        {
            OriginalDamage = damage;
            Damage = damage;
        }

        public void ChangeDamage(int value)
        {
            Damage = value;
        }

        public void IncreaseDamage(int value)
        {
            Damage += value;
        }

        public void DecreaseDamage(int value)
        {
            Damage -= value;
            if (Damage < 0) Damage = 0;
        }
        
        
        
        
    }
}

namespace Battle.UnitEvents
{
    public class StartUnitTurnEvent
    {
        public Unit Unit { get; }

        public StartUnitTurnEvent(Unit unit)
        {
            Unit = unit;
        }
    }

    public class EndUnitTurnEvent
    {
        public Unit Unit { get; }

        public EndUnitTurnEvent(Unit unit)
        {
            Unit = unit;
        }
    }

    public class UnitTakeDamageEvent
    {
        public Unit Unit { get; }
        public int StartHp { get; }
        public int Amount { get; }

        public UnitTakeDamageEvent(Unit unit, int startHp)
        {
            Unit = unit;
            StartHp = startHp;
            Amount = StartHp - Unit.CurrentHp;
        }
    }

    public class UnitHealDamageEvent
    {
        public Unit Unit { get; }
        public int StartHp { get; }
        public int Amount { get; }

        public UnitHealDamageEvent(Unit unit, int startHp)
        {
            Unit = unit;
            StartHp = startHp;
            Amount = Unit.CurrentHp - StartHp;
        }
    }

    public class UnitDeathEvent
    {
        public Unit Unit { get; }

        public UnitDeathEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}