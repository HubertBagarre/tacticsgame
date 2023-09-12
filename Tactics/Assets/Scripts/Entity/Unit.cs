using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    
    public class Unit : MonoBehaviour, BattleEntity
    {
        [field: SerializeField] public Tile Tile { get; private set; }
        [field: SerializeField] public int Team { get; private set; } //0 is player
        [field: SerializeField] public UnitStatsSO Stats { get; private set; }

        [field: Header("Current Flags")]
        [field: SerializeField]
        public bool IsActive { get; private set; }
        
        [field: SerializeField] public bool CanMove { get; private set; } = true;

        [field: Header("Current Stats")]
        [field: SerializeField]
        public UnitBehaviourSO Behaviour { get; private set; }
        
        [field: SerializeField]
        public int Movement { get; private set; }

        [field: SerializeField] public int MovementLeft { get; private set; }
        [field:SerializeField] public int Speed { get; protected set; }
        public float DecayRate => Speed / 100f;
        [field:SerializeField] public float DistanceFromTurnStart { get; protected set; }

        public Sprite Portrait => Stats.Portrait;
        
        public void InitUnit(Tile tile, int team, UnitStatsSO so)
        {
            Tile = tile;
            Team = team;
            Stats = so;

            Movement = so.BaseMovement;
            Speed = so.BaseSpeed;
            Behaviour = so.Behaviour;

            IsActive = true;

            tile.SetUnit(this);
        }

        public void InitEntityForBattle()
        {
            Movement = Stats.BaseMovement;
            Speed = Stats.BaseSpeed;
            Behaviour = Stats.Behaviour;
            
            Behaviour.InitBehaviour(this);
        }
        
        public void StartTurn()
        {
            MovementLeft = Movement;

            //apply effects
            
            EventManager.Trigger(new StartUnitTurnEvent(this));
            
            Behaviour.RunBehaviour(this);
        }

        public void EndTurn()
        {
            EventManager.Trigger(new EndUnitTurnEvent(this));
        }

        public void SetTile(Tile tile)
        {
            Tile.RemoveUnit();

            Tile = tile;

            Tile.SetUnit(this);
        }

        public void MoveUnit(List<Tile> path)
        {
            var unit = this;
            
            if (unit == null) return; //does the unit exist ?
            if (!path.Any()) return; // checks for valid path
            if (path.Any(tile => tile.HasUnit())) return; //does the path have any unit on it ?

            if (unit.Tile != null) unit.Tile.RemoveUnit();

            StartCoroutine(MoveAnimationRoutine());

            IEnumerator MoveAnimationRoutine()
            {
                EventManager.Trigger(new UnitMovementStartEvent(unit));

                foreach (var tile in path)
                {
                    yield return null;

                    unit.transform.position = tile.transform.position;

                    unit.MovementLeft--;
                    tile.SetUnit(unit);
                    unit.SetTile(tile);
                }

                EventManager.Trigger(new UnitMovementEndEvent(unit));
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
    
    public class UnitMovementStartEvent
    {
        public Unit Unit { get; }

        public UnitMovementStartEvent(Unit unit)
        {
            Unit = unit;
        }
    }

    public class UnitMovementEndEvent
    {
        public Unit Unit { get; }

        public UnitMovementEndEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}