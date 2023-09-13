using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using AbilityEvent;
    using InputEvent;
    
    public class AbilityManager : MonoBehaviour
    {
        [SerializeField] private TileManager tileManager;

        public static event Action<UnitAbilityInstance> OnUpdatedCastingAbility;
        private UnitAbilityInstance currentCastingAbilityInstance;

        private void Start()
        {
            currentCastingAbilityInstance = null;
            
            EventManager.AddListener<StartAbilitySelectionEvent>(StartAbilitySelection);
        }

        private void StartAbilitySelection(StartAbilitySelectionEvent ctx)
        {
            var cancel = (currentCastingAbilityInstance == ctx.Ability);
            
            if (currentCastingAbilityInstance != null) EventManager.Trigger(new EndAbilitySelectionEvent(true));
            
            if(cancel) return;

            currentCastingAbilityInstance = ctx.Ability;
            var caster = ctx.Caster;
            
            EventManager.AddListener<EndAbilitySelectionEvent>(TryCastAbility,true);
            
            EventManager.AddListener<ClickTileEvent>(SelectTile);
            
            OnUpdatedCastingAbility?.Invoke(currentCastingAbilityInstance);
            
            if (currentCastingAbilityInstance.SO.IsInstantCast) EventManager.Trigger(new EndAbilitySelectionEvent(false));

            void TryCastAbility(EndAbilitySelectionEvent selectionEvent)
            {
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);
                
                OnUpdatedCastingAbility?.Invoke(null);
                
                if (selectionEvent.Canceled)
                {
                    currentCastingAbilityInstance.ClearTileSelection();
                    currentCastingAbilityInstance = null;
                    return;
                }
                
                currentCastingAbilityInstance.CastAbility(caster);
            }

            void SelectTile(ClickTileEvent clickEvent)
            {
                var tile = clickEvent.Tile;
                
                if(tile == null) return;

                currentCastingAbilityInstance.AddTileToSelection(tile);
            }
        }
    }
}



namespace Battle.AbilityEvent
{
    public class StartAbilitySelectionEvent
    {
        public UnitAbilityInstance Ability { get; }
        public Unit Caster { get; }

        public StartAbilitySelectionEvent(UnitAbilityInstance ability,Unit caster)
        {
            Ability = ability;
            Caster = caster;
        }
    }

    public class EndAbilitySelectionEvent
    {
        public bool Canceled { get; }

        public EndAbilitySelectionEvent(bool canceled)
        {
            Canceled = canceled;
        }
    }

    public class StartAbilityCastEvent
    {
        public UnitAbilityInstance Ability { get; }
        public Unit Caster { get; }

        public StartAbilityCastEvent(UnitAbilityInstance ability,Unit caster)
        {
            Ability = ability;
            Caster = caster;
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
