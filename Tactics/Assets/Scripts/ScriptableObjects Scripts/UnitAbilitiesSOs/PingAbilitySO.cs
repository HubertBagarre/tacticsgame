using UnityEngine;

namespace Battle.ScriptableObjects
{
    using AbilityEvent;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log("Ping");
            
            EventManager.Trigger(new EndAbilityCastEvent());
        }
    }
}