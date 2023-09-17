using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using ScriptableObjects;
    using BattleEvents;
    
    public class Unit : MonoBehaviour, BattleEntity
    {
        [field: SerializeField] public Tile Tile { get; private set; }
        [field: SerializeField] public int Team { get; private set; } //0 is player
        [field: SerializeField] public UnitStatsSO Stats { get; private set; }
        public Sprite Portrait => Stats.Portrait;
        
        [field: Header("Current Flags")]
        [field: SerializeField] public bool IsActive { get; private set; }
        [field: SerializeField] public bool CanMove { get; private set; } = true;

        [field: Header("Current Stats")]
        [field: SerializeField] public UnitBehaviourSO Behaviour { get; private set; }
        [field: SerializeField] public int Movement { get; private set; }

        [field: SerializeField] public int MovementLeft { get; private set; }
        [field:SerializeField] public int Speed { get; protected set; }
        public float DecayRate => Speed / 100f;
        [field:SerializeField] public float DistanceFromTurnStart { get; protected set; }
        
        [field:SerializeField] public int CurrentHp { get; protected set; }
        public bool IsDead => CurrentHp <= 0;
        
        public List<UnitAbilityInstance> AbilityInstances { get; } = new ();

        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action OnDeath;

        public void InitUnit(Tile tile, int team, UnitStatsSO so)
        {
            Tile = tile;
            Team = team;
            Stats = so;

            Movement = so.BaseMovement;
            Speed = so.BaseSpeed;
            Behaviour = so.Behaviour;
            CurrentHp = so.MaxHp;

            IsActive = true;
            
            AbilityInstances.Clear();
            
            tile.SetUnit(this);
        }

        public void InitEntityForBattle()
        {
            Movement = Stats.BaseMovement;
            Speed = Stats.BaseSpeed;
            Behaviour = Stats.Behaviour;
            CurrentHp = Stats.MaxHp;

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

        public IEnumerator StartTurn()
        {
            MovementLeft = Movement;
            
            yield return null; //apply effects

            if (IsDead)
            {
                StartCoroutine(EndTurn());
                yield break;
            }

            OnTurnStart?.Invoke();
            
            yield return StartCoroutine(Behaviour.RunBehaviour(this));
            
            EventManager.Trigger(new StartUnitTurnEvent(this));
        }
        
        public IEnumerator EndTurn()
        {
            yield return null; //apply effects
            
            OnTurnEnd?.Invoke();
            
            EventManager.Trigger(new EndUnitTurnEvent(this));
        }

        public void SetTile(Tile tile)
        {
            Tile.RemoveUnit();

            Tile = tile;

            Tile.SetUnit(this);
        }

        public void MoveUnit(List<Tile> path,Action callback) //PATH DOESN'T INCLUDE STARTING TILE
        {
            if (!path.Any())
            {
                callback.Invoke();
                return; // checks for valid path
            }
            
            if (path.Any(tile => tile.HasUnit()))
            {
                return; //does the path have any unit on it ?
            }

            if (Tile != null) Tile.RemoveUnit();

            if (!CanContinueMovement())
            {
                callback.Invoke();
                return;
            }

            StartCoroutine(MoveAnimationRoutine());

            IEnumerator MoveAnimationRoutine()
            {
                for (var index = 0; index < path.Count && CanContinueMovement(); index++)
                {
                    var tile = path[index];
                    yield return new WaitForSeconds(1f);

                    transform.position = tile.transform.position;

                    MovementLeft--;
                    tile.SetUnit(this);
                    SetTile(tile);
                }

                callback.Invoke();
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
        
        public void TakeDamage(int amount)
        {
            if (amount < 0) amount = 0; //No negative damage, negative damage doesn't heal, but can deal 0 damage

            var startHp = CurrentHp;
            CurrentHp -= amount;
            
            EventManager.Trigger(new UnitTakeDamageEvent(this,startHp));
            
            if(CurrentHp <= 0) KillEntityInBattle();
        }

        [ContextMenu("Kill")]
        public void KillEntityInBattle()
        {
            Speed = 0;
            
            OnDeath?.Invoke();
            OnDeath = null;
            
            EventManager.Trigger(new UnitDeathEvent(this));
            
            gameObject.SetActive(false);
        }

        public void HealDamage(int amount)
        {
            if (amount < 0) amount = 0;

            var startHp = CurrentHp;
            CurrentHp += amount;
            
            EventManager.Trigger(new UnitHealDamageEvent(this,startHp));

            if (CurrentHp > Stats.MaxHp) CurrentHp = Stats.MaxHp; //No overheal (yet ?)
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
        
        public UnitTakeDamageEvent(Unit unit,int startHp)
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
        
        public UnitHealDamageEvent(Unit unit,int startHp)
        {
            Unit = unit;
            StartHp = startHp;
            Amount = Unit.CurrentHp - StartHp;
        }
    }

    public class UnitDeathEvent
    {
        public Unit Unit { get;}

        public UnitDeathEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}