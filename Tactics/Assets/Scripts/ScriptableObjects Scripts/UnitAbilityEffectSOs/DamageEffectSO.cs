using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Deal Damage")]
    public class DamageEffectSO : UnitAbilityEffectSO
    {
        [SerializeField] private int damage = 2;
        [SerializeField] private bool useCasterAttack = false;
        [SerializeField] private bool isAttack = false;
        
        public override string ConvertedDescription(Unit caster)
        {
            // TODO - damage type ?, maybe attack keyword?
            var dmg = useCasterAttack ? caster.Attack : damage;
            
            return isAttack ? $" <color=yellow><u><link=\"attack\">Attack</link></u></color> for <color=orange>{dmg} damage</color>." : $" Deal <color=orange>{dmg} damage</color>.";
        }
        
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = "Attack";
            
            return (linkKey == "attack" && isAttack);
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            //play animation
            var dmg = useCasterAttack ? caster.Attack : damage;
            
            yield return null;
            
            var target = targetTiles[0].Unit;

            if (isAttack)
            {
                yield return caster.AttackUnitEffect(target, dmg);
            }
            else
            {
                target.TakeDamage(dmg);
            }
            
            yield return null;
        }
    }
}