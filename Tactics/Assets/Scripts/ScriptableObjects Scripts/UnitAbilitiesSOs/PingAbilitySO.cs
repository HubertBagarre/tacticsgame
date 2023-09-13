using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        public override void CastAbility(Unit[] targetUnits, Tile[] targetTiles)
        {
            
        }
    }
}