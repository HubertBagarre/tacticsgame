using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Deal Damage")]
    public class DamageEffectSO : UnitAbilityEffectSO
    {
        [SerializeField] private int damage = 2;
        [SerializeField] private bool isAttack = false;
        
        public override string ConvertedDescription(Unit caster)
        {
            // TODO - damage type ?, maybe attack keyword?
            return isAttack ? $" Attack for <color=orange>{damage} damage</color>." : $" Deal <color=orange>{damage} damage</color>.";
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log($"Ping on {targetTiles[0]}");
            
            //play animation
            yield return null;
            
            var target = targetTiles[0].Unit;

            if (isAttack)
            {
                yield return caster.AttackUnitEffect(target, damage);
            }
            else
            {
                target.TakeDamage(damage);
            }
            
            yield return null;
        }
    }
}