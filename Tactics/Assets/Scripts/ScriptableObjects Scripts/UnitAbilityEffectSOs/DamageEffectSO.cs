using System.Collections;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Deal Damage")]
    public class DamageEffectSO : UnitAbilityEffectSO
    {
        [SerializeField,Tooltip("-1 is none")] private int hpDamage = 2;
        [SerializeField,Tooltip("-1 is none")] private int shieldDamage = 0;
        [SerializeField] private bool damageShieldFirst = false;
        [SerializeField] private bool useCasterAttack = false;
        [SerializeField] private bool isAttack = false;
        
        public override string ConvertedDescription(Unit caster)
        {
            // TODO - damage type ?, maybe attack keyword?
            var dmg = useCasterAttack ? caster.Stats.Attack : hpDamage;
            
            return isAttack ? $"<color=yellow><u><link=\"attack\">Attack</link></u></color>%AFFECTED% for <color=orange>{dmg} damage</color>." : $"Deal <color=orange>{dmg} damage</color>%toAFFECTED%.";
        }
        
        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = "Attack";
            
            return (linkKey == "attack" && isAttack);
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            //play animation
            var hpDmg = useCasterAttack ? caster.Stats.Attack : hpDamage;
            var damageInstance = new DamageInstance(hpDmg >= 0 ? hpDmg : null, shieldDamage >= 0 ? shieldDamage : null,damageShieldFirst);
            
            yield return null;

            foreach (var targetTile in targetTiles.Where(tile => tile.HasUnit()))
            {
                var target = targetTile.Unit;

                if (isAttack)
                {
                    yield return caster.AttackUnitEffect(target, damageInstance);
                }
                else
                {
                    target.TakeDamage(damageInstance);
                }
            }
            
            
            yield return null;
        }
    }
}