using System;
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
            
            EventManager.AddListener<StartAbilityTargetSelectionEvent>(StartAbilitySelection);
        }

        private void StartAbilitySelection(StartAbilityTargetSelectionEvent ctx)
        {
            // check if already casting ability
            // if yes and same ability cancel cast
            // if yes and differenet ability, cancel cast and start again with new ability
            
            var cancel = (currentCastingAbilityInstance == ctx.Ability);

            if (currentCastingAbilityInstance != null)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
                if (cancel) return;
            }
            
            currentCastingAbilityInstance = ctx.Ability;
            var caster = ctx.Caster;
            
            EventManager.AddListener<EndAbilityTargetSelectionEvent>(TryCastAbility,true);
            
            EventManager.AddListener<ClickTileEvent>(SelectTile);
            
            OnUpdatedCastingAbility?.Invoke(currentCastingAbilityInstance);

            if (currentCastingAbilityInstance.SO.IsInstantCast)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }

            void TryCastAbility(EndAbilityTargetSelectionEvent selectionEvent)
            {
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);
                
                OnUpdatedCastingAbility?.Invoke(null);
                
                if (selectionEvent.Canceled)
                {
                    currentCastingAbilityInstance.ClearTileSelection();
                    currentCastingAbilityInstance = null;
                    return;
                }

                var ability = currentCastingAbilityInstance;
                currentCastingAbilityInstance = null;
                
                ability.CastAbility(caster);
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
    public class StartAbilityTargetSelectionEvent
    {
        public UnitAbilityInstance Ability { get; }
        public Unit Caster { get; }

        public StartAbilityTargetSelectionEvent(UnitAbilityInstance ability,Unit caster)
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
