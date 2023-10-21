using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using UIEvents;
    using AbilityEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/PlayerUnit")]
    public class PlayerUnitBehaviourSO : UnitBehaviourSO
    {
        private Unit playerUnit;
        
        private bool isPlayerTurn;
        private Func<bool> IsPlayerTurn;
        private bool behaviourInterrupted;
        private bool isCastingAbility;

        public override void InitBehaviour(Unit unit)
        {
            playerUnit = unit;
            
            isPlayerTurn = false;
            behaviourInterrupted = false;
            isCastingAbility = false;
            IsPlayerTurn = () => isPlayerTurn;
            
            EventManager.AddListener<StartAbilityCastEvent>(EnterCastingAbility);
            EventManager.AddListener<EndAbilityCastEvent>(ExitCastingAbility);
        }

        public override void ShowBehaviourPreview(Unit unit)
        {
        }

        public override IEnumerator RunBehaviour(Unit unit)
        {
            isPlayerTurn = true;
            behaviourInterrupted = false;
            isCastingAbility = false;
            
            EventManager.Trigger(new StartPlayerControlEvent(unit));

            yield return new WaitWhile(IsPlayerTurn);
            
            EndPlayerTurn();
        }
        
        public override bool OnBehaviourInterrupted(Unit unit)
        {
            behaviourInterrupted = true;
            
            if (!isCastingAbility) isPlayerTurn = false;

            return false;
        }

        private void EndPlayerTurn()
        {
            EventManager.Trigger(new EndPlayerControlEvent());
        }

        private void EnterCastingAbility(StartAbilityCastEvent ctx)
        {
            if(ctx.Caster != playerUnit || !isPlayerTurn) return;
            isCastingAbility = true;
        }
        
        private void ExitCastingAbility(EndAbilityCastEvent ctx)
        {
            if(ctx.Caster != playerUnit || !isPlayerTurn) return;
            isCastingAbility = false;
            
            if (!ctx.Ability.EndUnitTurnAfterCast && !behaviourInterrupted) return;
            
            isPlayerTurn = false;
        }
    }
}


namespace Battle.UIEvents
{
    public class StartPlayerControlEvent
    {
        public Unit PlayerUnit { get; }

        public StartPlayerControlEvent(Unit playerUnit)
        {
            PlayerUnit = playerUnit;
        }
    }

    public class EndPlayerControlEvent
    {
    }
}