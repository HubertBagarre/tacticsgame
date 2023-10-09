using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using UIEvents;
    using UnitEvents;
    using AbilityEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/PlayerUnit")]
    public class PlayerUnitBehaviourSO : UnitBehaviourSO
    {
        private bool playerTurn;
        private Func<bool> IsPlayerTurn;

        public override void InitBehaviour(Unit unit)
        {
            playerTurn = false;
            IsPlayerTurn = () => playerTurn;
        }

        public override void ShowBehaviourPreview(Unit unit)
        {
        }

        public override IEnumerator RunBehaviour(Unit unit)
        {
            EventManager.AddListener<EndAbilityCastEvent>(EndTurnAfterAbilityCast);

            yield return null;
            
            Debug.Log("Starting player turn");
            playerTurn = true;
            EventManager.Trigger(new StartPlayerControlEvent(unit));

            yield return new WaitWhile(IsPlayerTurn);
            
            EventManager.RemoveListener<EndAbilityCastEvent>(EndTurnAfterAbilityCast);
        }
        
        public override void OnBehaviourInterrupted(Unit unit)
        {
            EventManager.RemoveListener<EndAbilityCastEvent>(EndTurnAfterAbilityCast);
            
            playerTurn = false;
            EventManager.Trigger(new EndPlayerControlEvent());
        }


        private void EndTurnAfterAbilityCast(EndAbilityCastEvent ctx)
        {
            if (!ctx.Ability.EndUnitTurnAfterCast) return;
            
            playerTurn = false;
            EventManager.Trigger(new EndPlayerControlEvent());
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