using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Special/Basic Attack")]
    public class BasicAttackAbilitySO : UnitAbilitySO
    {
        [SerializeField] private int range = 3;
        [SerializeField] private int critDamageMultiplier = 2;
        
        public override string ConvertedDescription(Unit caster)
        {
            var damage = caster.Attack;
            var crit = damage * critDamageMultiplier;

            var text = description.Replace("%range%", $"{range}")
                .Replace("%damage%",$"{damage}")
                .Replace("%crit%",$"{crit}");
            
            return text;
        }

        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            if (!selectableTile.HasUnit()) return false;

            if (selectableTile.Unit.Team == 0) return false;

            return caster.Tile.IsInSurroundingTileDistance(selectableTile, range);
        }

        protected override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            var damage = caster.Attack;
            var isCrit = caster.Tile.GetAdjacentTiles().Contains(targetTiles[0]);
            var target = targetTiles[0].Unit;
            
            if (isCrit) damage *= critDamageMultiplier;
            
            Debug.Log($"Attacking {target} for {damage} damage");
            
            //play unit attack animation (changes if isCrit or not)
            yield return null;
            
            target.TakeDamage(damage);
        }
    }
}


