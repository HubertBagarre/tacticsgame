using UnityEngine;

namespace Battle.ScriptableObjects
{
    using AbilityEvent;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log($"Ping on {targetTiles[0]} and {targetTiles[1]}");
            
            EventManager.Trigger(new EndAbilityCastEvent());
        }
    }
}