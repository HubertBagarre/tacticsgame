using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        [SerializeField] private int range = 2;
        [SerializeField] private int damage = 2;
        
        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return caster.Tile.IsInSurroundingTileDistance(selectableTile,range) && selectableTile.HasUnit();
        }

        protected override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log($"Ping on {targetTiles[0]}");
            
            //play animation
            yield return null;
            
            targetTiles[0].GetUnit().TakeDamage(damage);
            
            yield return null;
        }
    }
}