using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.UnitBehaviour
{
    using UIEvents;
    using AbilityEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/Unique/PlayerUnit")]
    public class PlayerUnitBehaviourSO : UnitBehaviourSO
    {
        private Func<bool> isInterruptedCondition;
        private bool endTurn;
        
        protected override void InitBehaviourEffect(Unit unit)
        {
            isInterruptedCondition = () => IsInterrupted && !IsCastingAbility && endTurn;
            endTurn = false;
            EventManager.AddListener<EndAbilityCastEvent>(InterruptOnCastingAbility);
        }

        public override void ShowBehaviourPreview()
        {
            
        }

        protected override IEnumerator RunBehaviourEffect()
        {
            endTurn = false;
            
            EventManager.Trigger(new StartPlayerControlEvent(AssociatedUnit));
            
            yield return new WaitUntil(isInterruptedCondition);
            
            EventManager.Trigger(new EndPlayerControlEvent());
        }
        
        protected override void OnBehaviourInterruptedEffect()
        {
            endTurn = true;
        }
        
        private void InterruptOnCastingAbility(EndAbilityCastEvent ctx)
        {
            if(ctx.Caster != AssociatedUnit) return;
            if (ctx.Ability.EndUnitTurnAfterCast)
            {
                AssociatedUnit.InterruptBehaviour();
            }
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