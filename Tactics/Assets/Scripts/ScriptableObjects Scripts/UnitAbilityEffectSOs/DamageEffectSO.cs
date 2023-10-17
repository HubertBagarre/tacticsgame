using System.Collections;
using System.Linq;
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
            var dmg = useCasterAttack ? caster.Stats.Attack : damage;
            
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
            var dmg = useCasterAttack ? caster.Stats.Attack : damage;
            
            yield return null;

            foreach (var targetTile in targetTiles.Where(tile => tile.HasUnit()))
            {
                var target = targetTile.Unit;

                if (isAttack)
                {
                    yield return caster.AttackUnitEffect(target, dmg);
                }
                else
                {
                    target.TakeDamage(dmg);
                }
            }
            
            
            yield return null;
        }
    }
}