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
        [field: SerializeField] public int AbilityPoints { get; private set; } = 4;
        public static int MaxAbilityPoints { get; } = 8;

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
            
            if (AbilityPoints - currentCastingAbilityInstance.Cost < 0)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
                return;
            }

            EventManager.AddListener<EndAbilityTargetSelectionEvent>(TryCastAbility, true);

            caster.OnDeath += CancelAbilityTargetSelection;

            EventManager.AddListener<ClickTileEvent>(SelectTile);

            OnUpdatedCastingAbility?.Invoke(caster, currentCastingAbilityInstance);

            if (currentCastingAbilityInstance.SO.SkipTargetSelection)
            {
                caster.OnDeath -= CancelAbilityTargetSelection;

                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }

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
            }
        }

        private void CastAbility(Unit unit, UnitAbilityInstance ability)
        {
            ability.CastAbility(unit);

            ConsumeAbilityPoints(ability.Cost);
        }

        private void InitAbilityPoints(StartRoundEvent ctx)
        {
            var startingAmount = -AbilityPoints;
            AbilityPoints = 0;
            ConsumeAbilityPoints(startingAmount);
        }

        public void ConsumeAbilityPoints(int amount)
        {
            var previous = AbilityPoints;
            AbilityPoints -= amount;
            if (AbilityPoints > MaxAbilityPoints)
            {
                //Trigger overload
                AbilityPoints = MaxAbilityPoints;
            }

            if (AbilityPoints < 0)
            {
                //Trigger underload(?)
                AbilityPoints = 0;
            }
            
            Debug.Log($"Updating Ability Points : {previous} to {AbilityPoints}");
            OnUpdatedAbilityPoints?.Invoke(previous,AbilityPoints);
        }
    }
}


namespace Battle.AbilityEvents
{
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

        public EndAbilityCastEvent(UnitAbilitySO ability)
        {
            Ability = ability;
        }
    }
}