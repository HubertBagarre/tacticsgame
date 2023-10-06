using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Area Selector")]
    public class AreaSelectorAbilitySO : UnitAbilitySO
    {
        public override List<Tile> GetAffectedTiles(Unit caster, Tile lastSelected, List<Tile> selectedTiles)
        {
            var tiles = lastSelected.GetSurroundingTiles();
            tiles.Add(lastSelected);
            
            return tiles;
        }

        protected override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                var unit = tile.Unit;
                if(unit != null) unit.TakeDamage(1);
            }
            
            
            yield return null;
        }
    }

}

