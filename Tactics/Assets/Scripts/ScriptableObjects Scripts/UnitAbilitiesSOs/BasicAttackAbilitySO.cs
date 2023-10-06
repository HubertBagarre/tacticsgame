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
            var damage = caster.Attack;
            var crit = damage * critDamageMultiplier;
            
            return $"Deal <color=orange>{damage} damage</color>. If the enemy is on an adjacent tile, deal <color=orange>{crit} damage</color> instead.";
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                if(!tile.HasUnit()) continue;
                
                var damage = caster.Attack;
                var isCrit = caster.Tile.GetAdjacentTiles().Contains(tile);
                var target = tile.Unit;
            
                if (isCrit) damage *= critDamageMultiplier;
            
                Debug.Log($"Attacking {target} for {damage} damage");
            
                //play unit attack animation (changes if isCrit or not)
                yield return caster.AttackUnitEffect(target, damage);
            }
            
        }
    }
}


