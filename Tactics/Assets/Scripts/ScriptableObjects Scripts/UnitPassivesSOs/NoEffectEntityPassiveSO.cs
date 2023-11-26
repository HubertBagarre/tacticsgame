using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectUnitPassiveSo : UnitPassiveSo
    {
        protected override void OnAddedEffect(PassiveInstance instance, int startingStacks)
        {
            
        }

        protected override void OnStacksAddedEffect(PassiveInstance instance, int amount)
        {
            
        }

        protected override void OnStacksRemovedEffect(PassiveInstance instance, int amount)
        {
            
        }

        protected override void OnRemovedEffect(PassiveInstance instance)
        {
            
        }

        protected override void TurnEndEffect(UnitTurnBattleAction action, PassiveInstance instance)
        {
            
        }

        protected override void TurnStartEffect(UnitTurnBattleAction action, PassiveInstance instance)
        {
            
        }
    }
}


