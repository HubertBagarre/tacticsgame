using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            
            
            Debug.Log($"Ping on {targetTiles[0]} and {targetTiles[1]}");
            
            EndAbility();
        }
    }
}