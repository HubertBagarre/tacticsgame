using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using ScriptableObjects;

    public class Unit : MonoBehaviour, IBattleEntity, IPassivesContainer<Unit>
    {
        [SerializeField] private UnitSO defaultUnitSo;
        
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
        [SerializeField] private float distanceFromTurnStart;
        public float DistanceFromTurnStart
        {
            get => distanceFromTurnStart;
            protected set
            {
                distanceFromTurnStart = value;
                OnDistanceFromTurnStartChanged?.Invoke(this);
            }
        }
        public static event Action<IBattleEntity> OnDistanceFromTurnStartChanged;
        public bool IsDead => Stats.CurrentHp <= 0;
        private event Action<bool> OnBreakChanged; 
        private bool isBreak;
        public bool IsBreak
        {
            get => isBreak;
            private set
            {
                var changed = isBreak != value;
                isBreak = value;
                if(changed) OnBreakChanged?.Invoke(isBreak);
            } 
        }

        [field: SerializeField] public int CurrentUltimatePoints { get; protected set; }
        public int MaxUltimatePoints => GetHighestCostUltimate();
        public event Action<int, int> OnUltimatePointsAmountChanged;
        
        public List<UnitAbilityInstance> AbilityInstances { get; } = new();
        public List<PassiveInstance<Unit>> PassiveInstances { get; } = new();

        private List<IEnumerator> onAttackOtherUnitRoutines = new ();
        private List<IEnumerator> onAttackedRoutines = new ();

        private UnitBehaviourSO Behaviour => Stats.Behaviour;
        private Coroutine behaviourRoutine;
        
        // TODO - use IEnumerator delegates instead of action;
        public event IBattleEntity.BattleEntityDelegate OnTurnStart;
        public event IBattleEntity.BattleEntityDelegate OnTurnEnd;
        public event Action OnDeath;
        private List<IPassivesContainer<Unit>.PassiveInstanceDelegate> PassiveAddedCallbacks { get; } = new();
        private List<IPassivesContainer<Unit>.PassiveInstanceDelegate>  PassiveRemovedCallbacks { get; } = new();
        public delegate IEnumerator TileDelegate(Unit unit,Tile tile);
        public event TileDelegate OnTileEnter;
        public event TileDelegate OnTileExit;
        public static event Action<Unit> OnUnitInit;

        public IEnumerator InitUnit(Tile tile, int team, UnitSO so,Tile.Direction orientation)
        {
            // TODO - Instantiate model
            if (so == null) so = defaultUnitSo;
            
            var model = so.model;
            if (model == null) model = defaultUnitSo.model;
            
            BattleModel = Instantiate(model, modelParent);
            BattleModel.SetOrientation(orientation);
            BattleModel.SetPosition(transform.position);
            BattleModel.ShowGhost(false);
            
            Tile = tile;
            Team = team;
            Stats = so.CreateInstance(this);
            
            PassiveAddedCallbacks.Clear();
            PassiveRemovedCallbacks.Clear();
            
            if(Tile != null) yield return StartCoroutine( tile.SetUnit(this));
            
            OnUnitInit?.Invoke(this);
        }

        public void InitEntityForBattle()
        {
            CurrentUltimatePoints = 0;
            
            Stats.ResetModifiers();
            
            IsActive = true;
            IsBreak = false;

            AbilityInstances.Clear();
            foreach (var abilityToAdd in Stats.So.Abilities)
            {
                AbilityInstances.Add(abilityToAdd.CreateInstance());
            }
            behaviourRoutine = null;
            
            onAttackOtherUnitRoutines.Clear();
        }

        public IEnumerator LateInitEntityForBattle()
        {
            foreach (var passiveToAdd in Stats.So.StartingPassives)
            {
                yield return StartCoroutine(AddPassiveEffect(passiveToAdd.UnitPassive, passiveToAdd.UnitStacks));
            }
        }

        public void PreStartRound()
        {
            if (isBreak)
            {
                if (Stats.CurrentShield <= 0) Stats.CurrentShield = Stats.MaxShield;
            }
        }

        public IEnumerator StartRound()
        {
            yield return null; //apply effects
        }

        public IEnumerator EndRound()
        {
            yield return null; //apply effects
        }   

        public void FastForwardTurn()
        {
            DistanceFromTurnStart = 0;
        }
        
        public void SkipTurn(bool interruptBehaviour)
        {
            IBattleEntity entity = BattleManager.EndRoundEntity;
            if(entity == null) return;
            
            DistanceFromTurnStart = (entity.DistanceFromTurnStart+0.01f) * Speed / entity.Speed;

            if (interruptBehaviour) InterruptBehaviour();
        }

        public void InterruptBehaviour()
        {
            if (behaviourRoutine == null)  return;

            Debug.Log("Interrupting behaviour");

            Behaviour.OnBehaviourInterrupted();
        }

        public IEnumerator StartTurn(Action onBehaviourEnd)
        {
            MovementLeft = Stats.Movement;
            
            foreach (var abilityInstance in AbilityInstances)
            {
                abilityInstance.DecreaseCurrentCooldown(1);
            }

            if (OnTurnStart != null) yield return StartCoroutine(OnTurnStart?.Invoke(this));
            
            EventManager.Trigger(new StartUnitTurnEvent(this));

            UIBattleManager.OnEndTurnButtonClicked += InterruptBehaviour;

            var behaviourRunning = true;

            StartCoroutine(RunBehaviour());

            yield return new WaitWhile(IsBehaviourRoutineRunning);

            UIBattleManager.OnEndTurnButtonClicked -= InterruptBehaviour;

            onBehaviourEnd.Invoke();
            yield break;

            bool IsBehaviourRoutineRunning() => behaviourRoutine != null && behaviourRunning;

            IEnumerator RunBehaviour()
            {
                behaviourRoutine = StartCoroutine(Behaviour.RunBehaviour());
                yield return behaviourRoutine;
                behaviourRoutine = null;
                behaviourRunning = false;
            }
        }

        public IEnumerator EndTurn()
        {
            if (OnTurnEnd != null) yield return StartCoroutine(OnTurnEnd?.Invoke(this));

            EventManager.Trigger(new EndUnitTurnEvent(this));
        }

        public IEnumerator SetTile(Tile tile)
        {
            if (Tile != null)
            {
                if(OnTileExit != null) yield return StartCoroutine(OnTileExit?.Invoke(this,Tile));
                yield return StartCoroutine(Tile.RemoveUnit());
            }

            Tile = tile;
            
            if(OnTileEnter != null) yield return StartCoroutine(OnTileEnter?.Invoke(this,Tile));
            yield return StartCoroutine(Tile.SetUnit(this));
        }

        public IEnumerator TeleportUnit(Tile tile,bool isForced)
        {
            if(tile == null) yield break;
            
            
            //TODO - play animation, different is isForced
            yield return new WaitForSeconds(0.5f);
            
            transform.position = tile.transform.position;
            BattleModel.SetPosition(transform.position);
            yield return StartCoroutine(SetTile(tile));
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
            
            Debug.Log($"Moving {path.Count} tiles");
            for (var index = 0; index < path.Count && CanContinueMovement(); index++)
            {
                var tile = path[index];
                
                //TODO - play animation, different is isForced
                yield return new WaitForSeconds(0.5f);

                if(!isForced) MovementLeft--;
                
                transform.position = tile.transform.position;
                BattleModel.SetPosition(transform.position);
                yield return StartCoroutine(SetTile(tile));
            }

            yield break;

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

        public void DecreaseMovement(int value)
        {
            MovementLeft -= value;
            if(MovementLeft < 0) MovementLeft = 0;
        }

        public IEnumerator AttackUnitEffect(Unit attackedUnit,DamageInstance damageInstance)
        {
            foreach (var routine in onAttackOtherUnitRoutines)
            {
                yield return StartCoroutine(routine);
                if(IsDead) yield break;
            }
            
            yield return StartCoroutine(attackedUnit.AttackedUnitEffect(damageInstance));
        }

        public IEnumerator AttackedUnitEffect(DamageInstance damageInstance)
        {
            foreach (var routine in onAttackedRoutines)
            {
                yield return StartCoroutine(routine);
            }
            TakeDamage(damageInstance);

            yield return new WaitForSeconds(1f);
        }

        public void TakeShieldDamage(DamageInstance damageInstance)
        {
            if(!damageInstance.TookShieldDamage) return;
            
            var amount = damageInstance.ShieldDamage ?? 0;
            if (amount < 0) amount = 0;
            
            Stats.CurrentShield -= amount;
            
            EventManager.Trigger(new UnitTakeDamageEvent(this, Stats.CurrentHp));

            if (Stats.CurrentShield <= 0) BreakShield();
        }

        public void TakeHpDamage(int amount,bool damageShieldFirst = false)
        {
            var instance = new DamageInstance(amount, null, damageShieldFirst);
            TakeHpDamage(instance);
        }
        
        public void TakeHpDamage(DamageInstance damageInstance)
        {
            if(!damageInstance.TookHPDamage) return;
            
            var amount = damageInstance.HpDamage ?? 0;
            if (amount < 0) amount = 0; //No negative damage, negative damage doesn't heal, but can deal 0 damage
            if(isBreak) amount *= 2;
            
            var startHp = Stats.CurrentHp;
            Stats.CurrentHp -= amount;
            
            EventManager.Trigger(new UnitTakeDamageEvent(this, startHp));

            if (Stats.CurrentHp <= 0) KillEntityInBattle();
        }

        public void TakeDamage(DamageInstance damageInstance)
        {
            if (damageInstance.DamageShieldFirst)
            {
                if(damageInstance.TookShieldDamage) TakeShieldDamage(damageInstance);
                if(damageInstance.TookHPDamage) TakeHpDamage(damageInstance);
                return;
            }
            
            if(damageInstance.TookHPDamage) TakeHpDamage(damageInstance);
            if(damageInstance.TookShieldDamage) TakeShieldDamage(damageInstance);
        }

        public void HealDamage(int amount)
        {
            if (amount < 0) amount = 0;

            var startHp = Stats.CurrentHp;
            Stats.CurrentHp += amount;

            EventManager.Trigger(new UnitHealDamageEvent(this, startHp));
        }

        public void BreakShield()
        {
            if(Stats.CurrentShield > 0) Stats.CurrentShield = 0;
            IsBreak = true;
            SkipTurn(true);
        }
        
        
        [ContextMenu("Execute")]
        public void Execute()
        {
            KillEntityInBattle();
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

        public void AddOnPassiveAddedCallback(IPassivesContainer<Unit>.PassiveInstanceDelegate callback)
        {
            PassiveAddedCallbacks.Add(callback);
            Debug.Log($"Added callback, now {PassiveAddedCallbacks.Count} callbacks");
        }

        public void AddOnPassiveRemovedCallback(IPassivesContainer<Unit>.PassiveInstanceDelegate callback)
        {
            PassiveRemovedCallbacks.Add(callback);
        }

        public void RemoveOnPassiveAddedCallback(IPassivesContainer<Unit>.PassiveInstanceDelegate callback)
        {
            if (!PassiveAddedCallbacks.Contains(callback)) return;
            
            PassiveAddedCallbacks.Remove(callback);
            Debug.Log($"Removed callback, now {PassiveAddedCallbacks.Count} callbacks");
        }

        public void RemoveOnPassiveRemovedCallback(IPassivesContainer<Unit>.PassiveInstanceDelegate callback)
        {
            if(PassiveRemovedCallbacks.Contains(callback)) PassiveRemovedCallbacks.Remove(callback);
        }

        public TPassiveInstance GetPassiveInstance<TPassiveInstance>(PassiveSO<Unit> passiveSo) where TPassiveInstance : PassiveInstance<Unit>
        {
            var instance = PassiveInstances.FirstOrDefault(passiveInstance => passiveInstance.SO == passiveSo);
            
            return instance as TPassiveInstance;
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveSo"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public IEnumerator AddPassiveEffect(PassiveSO<Unit> passiveSo, int amount = 1)
        {
            //add passive instance to list
            if (!passiveSo.IsStackable || (passiveSo.IsStackable && amount <= 0)) amount = 1;
            
            var passiveInstance = GetPassiveInstance<PassiveInstance<Unit>>(passiveSo);

            return AddToPassives(passiveInstance);
            
            IEnumerator AddToPassives(PassiveInstance<Unit> instance)
            {
                //if current instance == null, no passive yet, creating new and adding to list
                //if passive isn't stackable, creating new and adding to list
                if (instance == null || !passiveSo.IsStackable)
                {
                    instance = passiveSo.CreateInstance<PassiveInstance<Unit>>(amount);
                    PassiveInstances.Add(instance);
                }

                var callbacks = PassiveAddedCallbacks.ToList();
                Debug.Log($"Found {callbacks.Count} callbacks");
                foreach (var callback in callbacks)
                {
                    yield return StartCoroutine(callback.Invoke(instance));
                }
                
                yield return StartCoroutine(instance.AddPassive(this));
            }
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveSo"></param>
        /// <returns></returns>
        public IEnumerator RemovePassiveEffect(PassiveSO<Unit> passiveSo)
        {
            var currentInstance = GetPassiveInstance<PassiveInstance<Unit>>(passiveSo);
            return currentInstance == null ? null : RemovePassiveEffect(currentInstance);
        }

        /// <summary>
        /// CAN RETURN NULL
        /// </summary>
        /// <param name="passiveInstance"></param>
        /// <returns></returns>
        public IEnumerator RemovePassiveEffect(PassiveInstance<Unit> passiveInstance)
        {
            if (!PassiveInstances.Contains(passiveInstance)) yield break;
            PassiveInstances.Remove(passiveInstance);

            var callbacks = PassiveRemovedCallbacks.ToList();
            foreach (var callback in callbacks)
            {
                yield return StartCoroutine(callback.Invoke(passiveInstance));
            }
            
            yield return StartCoroutine(passiveInstance.RemovePassive(this));
        }
        
        public int GetPassiveEffectCount(Func<PassiveInstance<Unit>,bool> condition,out PassiveInstance<Unit> firstPassiveInstance)
        {
            condition ??= _ => true;
            
            firstPassiveInstance = PassiveInstances.Where(condition).FirstOrDefault();
            
            return PassiveInstances.Count(condition);
        }
    }
    
    public class DamageInstance
    {
        public int? OriginalHpDamage { get; }
        public int? OriginalShieldDamage { get; }
        public int? HpDamage { get; private set; }
        public int? ShieldDamage { get; private set; }
        public bool TookHPDamage => HpDamage != null;
        public bool TookShieldDamage => ShieldDamage != null;
        public bool DamageShieldFirst { get; }

        public DamageInstance(int? hpDamage, int? shieldDamage, bool damageShieldFirst = false)
        {
            OriginalHpDamage = hpDamage;
            HpDamage = hpDamage;
            
            OriginalShieldDamage = shieldDamage;
            ShieldDamage = shieldDamage;

            DamageShieldFirst = damageShieldFirst;
        }

        public override string ToString()
        {
            return $"AttackInstance: {OriginalHpDamage} -> {HpDamage}, {OriginalShieldDamage} -> {ShieldDamage}";
        }

        public void ChangeHpDamage(int value)
        {
            HpDamage = value;
        }

        public void IncreaseHpDamage(int value)
        {
            if(!TookHPDamage) return;
            HpDamage += value;
        }

        public void DecreaseHpDamage(int value)
        {
            if(!TookHPDamage) return;
            HpDamage -= value;
            if (HpDamage < 0) HpDamage = 0;
        }
        
        public void ChangeShieldDamage(int value)
        {
            ShieldDamage = value;
        }

        public void IncreaseShieldDamage(int value)
        {
            if(!TookShieldDamage) return;
            ShieldDamage += value;
        }

        public void DecreaseShieldDamage(int value)
        {
            if(!TookShieldDamage) return;
            ShieldDamage -= value;
            if (ShieldDamage < 0) ShieldDamage = 0;
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