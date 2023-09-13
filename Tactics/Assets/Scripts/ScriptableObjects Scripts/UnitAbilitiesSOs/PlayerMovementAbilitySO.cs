using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Special/PlayerMovement")]
    public class PlayerMovementAbilitySO : UnitAbilitySO
    {
        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log("Movement ability effect");
        }
    }
}


