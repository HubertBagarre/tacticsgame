using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Basic Attack")]
    public class BasicAttackAbilitySO : UnitAbilityEffectSO
    {
        [SerializeField] private int critDamageMultiplier = 2;
        
        public override string ConvertedDescription(Unit caster)
        {
            var damage = caster.Stats.Attack;
            var crit = damage * critDamageMultiplier;
            
            return $"<color=yellow><u><link=\"attack\">Attack</link></u></color>%AFFECTED% for <color=orange>{damage} damage</color>. If the enemy is on an adjacent tile, " +
                   $"<color=yellow><u><link=\"attack\">attack</link></u></color>%AFFECTED% for <color=orange>{crit} damage</color> instead.";
        }

        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = "Attack";
            
            return (linkKey == "attack");
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                if(!tile.HasUnit()) continue;
                
                var damage = caster.Stats.Attack;
                var isCrit = caster.Tile.GetAdjacentTiles().Contains(tile);
                var target = tile.Unit;
            
                if (isCrit) damage *= critDamageMultiplier;
                
                //play unit attack animation (changes if isCrit or not)
                yield return caster.AttackUnitEffect(target, damage);
            }
            
        }
    }
}


