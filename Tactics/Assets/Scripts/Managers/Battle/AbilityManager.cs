using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using AbilityEvent;
    
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
            
            Debug.Log($"Starting Selection for {ability}");
        }
    }
}



namespace Battle.AbilityEvent
{
    public class StartAbilitySelectionEvent
    {
        public UnitAbilitySO Ability { get; }

        public StartAbilitySelectionEvent(UnitAbilitySO ability)
        {
            Ability = ability;
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
    
    public class StartAbilityCast{}
    
    public class EndAbilityCast{}
}
