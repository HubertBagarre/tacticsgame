using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Teleport Unit")]
    public class BlinkEffectSO : UnitAbilityEffectSO
    {
        public override string ConvertedDescription(Unit caster)
        {
            return "Blink to tile";
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                yield return caster.StartCoroutine(caster.TeleportUnit(tile, false));
            }
        }
    }
}