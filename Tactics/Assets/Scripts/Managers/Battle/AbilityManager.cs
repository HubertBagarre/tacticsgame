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

        private void Start()
        {
            EventManager.AddListener<StartAbilitySelectionEvent>(StartAbilitySelection);
        }

        private void StartAbilitySelection(StartAbilitySelectionEvent ctx)
        {
            var ability = ctx.Ability;
            var caster = ctx.Caster;
            
            EventManager.AddListener<EndAbilitySelectionEvent>(TryCastAbility,true);
            
            // add listener on click to add tile to ability instance
            EventManager.AddListener<ClickTileEvent>(SelectTile);
            
            if (ability.SO.IsInstantCast)
            {
                EventManager.Trigger(new EndAbilitySelectionEvent(false));
                return;
            }

            void TryCastAbility(EndAbilitySelectionEvent selectionEvent)
            {
                EventManager.RemoveListener<ClickTileEvent>(SelectTile);

                if (selectionEvent.Canceled)
                {
                    ability.ClearTileSelection();
                    return;
                }
                
                ability.CastAbility(caster);
            }

            void SelectTile(ClickTileEvent clickEvent)
            {
                var tile = clickEvent.Tile;
                
                if(tile == null) return;

                ability.AddTileToSelection(tile);
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
    
    public class StartAbilityCastEvent{}
    
    public class EndAbilityCastEvent{}
}
