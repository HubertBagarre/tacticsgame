using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using AbilityEvents;
    using InputEvent;
    using BattleEvents;

    public class AbilityManager : MonoBehaviour
    {
        [field: SerializeField] public int AbilityPoints { get; private set; } = 3;
        public static int MaxAbilityPoints { get; } = 6;

        public static event Action<Unit, UnitAbilityInstance> OnUpdatedCastingAbility;
        public static event Action<int, int> OnUpdatedAbilityPoints;

        private Unit caster;
        private UnitAbilityInstance currentCastingAbilityInstance;

        private void Awake()
        {
            OnUpdatedCastingAbility = null;
            OnUpdatedAbilityPoints = null;
        }

        private void Start()
        {
            currentCastingAbilityInstance = null;
            caster = null;
            
            EventManager.AddListener<StartRoundEvent>(InitAbilityPoints,true);

            EventManager.AddListener<StartAbilityTargetSelectionEvent>(StartAbilitySelection);
        }

        private void StartAbilitySelection(StartAbilityTargetSelectionEvent ctx)
        {
            if (currentCastingAbilityInstance != null)
            {
                var cancel = (currentCastingAbilityInstance == ctx.Ability);

                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));

                if (cancel) return;

                EventManager.Trigger(ctx);

                return;
            }

            currentCastingAbilityInstance = ctx.Ability;
            caster = ctx.Caster;

            currentCastingAbilityInstance.ClearSelection();
            
            if (caster.CurrentUltimatePoints < currentCastingAbilityInstance.UltimateCost)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
                return;
            }

            EventManager.AddListener<EndAbilityTargetSelectionEvent>(TryCastAbility, true);

            caster.OnDeath += CancelAbilityTargetSelection;

            EventManager.AddListener<ClickTileEvent>(SelectTile);

            currentCastingAbilityInstance.StartTileSelection(caster);
            
            if (currentCastingAbilityInstance.SO.SkipTargetSelection)
            {
                currentCastingAbilityInstance.AddTileToSelection(caster,caster.Tile,true);
                caster.OnDeath -= CancelAbilityTargetSelection;
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);
                
                //EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }
            
            OnUpdatedCastingAbility?.Invoke(caster, currentCastingAbilityInstance);
            return;

            void CancelAbilityTargetSelection()
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
            }

            void TryCastAbility(EndAbilityTargetSelectionEvent selectionEvent)
            {
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);

                OnUpdatedCastingAbility?.Invoke(caster, null);

                var ability = currentCastingAbilityInstance;
                var unit = caster;
                currentCastingAbilityInstance = null;
                caster = null;

                if (selectionEvent.Canceled)
                {
                    //currentCastingAbilityInstance.ClearTileSelection();
                    return;
                }

                CastAbility(unit, ability);
            }

            void SelectTile(ClickTileEvent clickEvent)
            {
                var tile = clickEvent.Tile;

                if (tile == null) return;
                
                currentCastingAbilityInstance.AddTileToSelection(caster, tile);
                
                OnUpdatedCastingAbility?.Invoke(caster, currentCastingAbilityInstance);
            }
        }

        private void CastAbility(Unit unit, UnitAbilityInstance ability)
        {
            if(ability.IsUltimate) unit.ConsumeUltimatePoint(ability.UltimateCost);
            
            if (ability.SO.Cooldown > 0) ability.EnterCooldown();
            
            if(!ability.IsUltimate) unit.GainUltimatePoint(1);
            
            ability.CastAbility(unit);
            
            ConsumeAbilityPoints(unit,ability.Cost);
        }

        private void InitAbilityPoints(StartRoundEvent ctx)
        {
            var startingAmount = -AbilityPoints;
            AbilityPoints = 0;
            ConsumeAbilityPoints(null,startingAmount);
        }

        public void ConsumeAbilityPoints(Unit unit,int amount)
        {
            if(unit != null) if(unit.Team != 0) return;
            
            var previous = AbilityPoints;
            AbilityPoints -= amount;
            if (AbilityPoints > MaxAbilityPoints)
            {
                Overload(unit);
            }

            if (AbilityPoints < 0)
            {
                Underload(unit);
            }
            
            OnUpdatedAbilityPoints?.Invoke(previous,AbilityPoints);
        }

        public void IncreaseUnitUltimatePoints()
        {
            
        }

        public void Overload(Unit unit)
        {
            Debug.Log("Overload");
            unit.BreakShield();
            AbilityPoints = 0;
        }

        public void Underload(Unit unit)
        {
            AbilityPoints = 0;
        }
    }
}


namespace Battle.AbilityEvents
{
    using ScriptableObjects;
    
    public class StartAbilityTargetSelectionEvent
    {
        public UnitAbilityInstance Ability { get; }
        public Unit Caster { get; }

        public StartAbilityTargetSelectionEvent(UnitAbilityInstance ability, Unit caster)
        {
            Ability = ability;
            Caster = caster;
        }
    }

    public class EndAbilityTargetSelectionEvent
    {
        public bool Canceled { get; }

        public EndAbilityTargetSelectionEvent(bool canceled)
        {
            Canceled = canceled;
        }
    }

    public class StartAbilityCastEvent
    {
        public UnitAbilityInstance Ability { get; }
        public Unit Caster { get; }
        public List<Tile> SelectedTiles { get; }

        public StartAbilityCastEvent(UnitAbilityInstance ability, Unit caster, List<Tile> selectedTiles)
        {
            Ability = ability;
            Caster = caster;
            SelectedTiles = selectedTiles;
        }
    }

    public class EndAbilityCastEvent
    {
        public UnitAbilitySO Ability { get; }
        public Unit Caster { get; }

        public EndAbilityCastEvent(UnitAbilitySO ability,Unit caster)
        {
            Ability = ability;
            Caster = caster;
        }
    }
}