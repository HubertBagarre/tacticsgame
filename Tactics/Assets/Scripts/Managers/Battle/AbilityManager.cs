using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using AbilityEvent;
    using InputEvent;
    
    public class AbilityManager : MonoBehaviour
    {
        // TODO - Remove if unused
        [SerializeField] private TileManager tileManager;

        public static event Action<Unit,UnitAbilityInstance> OnUpdatedCastingAbility;
        private UnitAbilityInstance currentCastingAbilityInstance;

        private void Start()
        {
            currentCastingAbilityInstance = null;
            
            EventManager.AddListener<StartAbilityTargetSelectionEvent>(StartAbilitySelection);
        }

        private void StartAbilitySelection(StartAbilityTargetSelectionEvent ctx)
        {
            if (currentCastingAbilityInstance != null)
            {
                var cancel = (currentCastingAbilityInstance == ctx.Ability);
                
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(true));
                
                if(cancel) return;
                
                EventManager.Trigger(ctx);
                
                return;
            }
            
            currentCastingAbilityInstance = ctx.Ability;
            var caster = ctx.Caster;
            
            EventManager.AddListener<EndAbilityTargetSelectionEvent>(TryCastAbility,true);
            
            EventManager.AddListener<ClickTileEvent>(SelectTile);
            
            OnUpdatedCastingAbility?.Invoke(caster,currentCastingAbilityInstance);

            if (currentCastingAbilityInstance.SO.SkipTargetSelection)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }

            void TryCastAbility(EndAbilityTargetSelectionEvent selectionEvent)
            {
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);
                
                OnUpdatedCastingAbility?.Invoke(caster,null);
                
                var ability = currentCastingAbilityInstance;
                currentCastingAbilityInstance = null;
                
                if (selectionEvent.Canceled)
                {
                    //currentCastingAbilityInstance.ClearTileSelection();
                    return;
                }
                
                ability.CastAbility(caster);
            }

            void SelectTile(ClickTileEvent clickEvent)
            {
                var tile = clickEvent.Tile;
                
                if(tile == null) return;

                currentCastingAbilityInstance.AddTileToSelection(caster,tile);
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
        public List<Tile> SelectedTiles { get; }

        public StartAbilityCastEvent(UnitAbilityInstance ability,Unit caster,List<Tile> selectedTiles)
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
