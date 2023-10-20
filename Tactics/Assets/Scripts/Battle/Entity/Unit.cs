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
        [Header("Anchors")]
        [SerializeField] private Transform modelParent;
        [field:SerializeField] public Transform UiParent { get; private set; }
        public BattleModel BattleModel { get; private set; }
        
        [field: Header("Current Flags")]
        [field: SerializeField] public Tile Tile { get; private set; }
        [field: SerializeField] public int Team { get; private set; } //0 is player
        [field: SerializeField] public bool IsActive { get; private set; }

        [field: SerializeField] public bool CanMove { get; private set; } = true;

        public UnitStatsInstance Stats { get; private set; }
        public Sprite Portrait => Stats.So.Portrait;
        
        [Header("Current Stats")]
        [SerializeField] private int movementLeft;
        
        public int MovementLeft
        {
            get => movementLeft;
            private set
            {
                movementLeft = value;
                OnMovementLeftChanged?.Invoke(movementLeft);
            }
        }

        public event Action<int> OnMovementLeftChanged;

        public int Speed => Stats.Speed;
        public float DecayRate => Speed / 100f;
        [field: SerializeField] public float DistanceFromTurnStart { get; protected set; }
        public bool IsDead => Stats.CurrentHp <= 0;

        [field: SerializeField] public int CurrentUltimatePoints { get; protected set; }
        public int MaxUltimatePoints => GetHighestCostUltimate();
        public event Action<int, int> OnUltimatePointsAmountChanged;
        
        public List<UnitAbilityInstance> AbilityInstances { get; } = new();
        public List<UnitPassiveInstance> PassiveInstances { get; } = new();
        private List<UnitPassiveInstance> passivesToRemove = new();

        private List<IEnumerator> onAttackOtherUnitRoutines = new ();
        private List<IEnumerator> onAttackedRoutines = new ();

        private UnitBehaviourSO Behaviour => Stats.Behaviour;
        private Coroutine behaviourRoutine;
        
        
        // TODO - use IEnumerator delegates instead of action;
        public event Action<int> OnCurrentHealthChanged;
        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action OnDeath;
        public event Action<UnitPassiveInstance> OnPassiveAdded; 
        public event Action<UnitPassiveInstance> OnPassiveRemoved;

        public static event Action<Unit> OnUnitInit;

        public void InitUnit(Tile tile, int team, UnitSO so,Tile.Direction orientation)
        {
            // TODO - Instantiate model
            BattleModel = Instantiate(so.model, modelParent);
            BattleModel.SetOrientation(orientation);
            BattleModel.SetPosition(transform.position);
            BattleModel.ShowGhost(false);
            
            Tile = tile;
            Team = team;
            Stats = so.CreateInstance(this);
            
            tile.SetUnit(this);
            
            OnUnitInit?.Invoke(this);
        }

        public void InitEntityForBattle()
        {
            CurrentUltimatePoints = 0;
            
            Stats.ResetModifiers();
            
            IsActive = true;

            AbilityInstances.Clear();
            foreach (var ability in Stats.So.Abilities)
            {
                AbilityInstances.Add(ability.CreateInstance());
            }
            behaviourRoutine = null;
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

            Behaviour.OnBehaviourInterrupted(this);

            StopCoroutine(behaviourRoutine);
            behaviourRoutine = null;
        }

        public IEnumerator StartTurn(Action onBehaviourEnd)
        {
            MovementLeft = Stats.Movement;
            
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
                yield return new WaitForSeconds(0.5f);

                if(!isForced) MovementLeft--;
                transform.position = tile.transform.position;
                
                BattleModel.SetPosition(transform.position);
                
                SetTile(tile);
            }

            bool CanContinueMovement()
            {
                return MovementLeft > 0 && !IsDead;
            }
        }

        public void ResetTurnValue(float value)
        {
            if(Stats == null) return;
            DistanceFromTurnStart = value < 0 ? Stats.So.Initiative : value;
        }

        public void DecayTurnValue(float amount)
        {
            DistanceFromTurnStart -= amount * DecayRate;
        }

        public void SetMovement(int value)
        {
            MovementLeft = value;
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

            var startHp = Stats.CurrentHp;
            Stats.CurrentHp -= amount;
            
            EventManager.Trigger(new UnitTakeDamageEvent(this, startHp));

            if (Stats.CurrentHp <= 0) KillEntityInBattle();
            OnCurrentHealthChanged?.Invoke(Stats.CurrentHp);
        }

        public void HealDamage(int amount)
        {
            if (amount < 0) amount = 0;

            var startHp = Stats.CurrentHp;
            Stats.CurrentHp += amount;

            EventManager.Trigger(new UnitHealDamageEvent(this, startHp));
            
            OnCurrentHealthChanged?.Invoke(Stats.CurrentHp );
        }
        
        [ContextMenu("Execute")]
        public void Execute()
        {
            TakeDamage(Stats.CurrentHp);
        }

        public void KillEntityInBattle()
        {
            if(Tile != null) Tile.RemoveUnit();

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

        public override string ToString()
        {
            return $"AttackInstance: {OriginalDamage} -> {Damage}";
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
            Amount = StartHp - Unit.Stats.CurrentHp;
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
            Amount = Unit.Stats.CurrentHp - StartHp;
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