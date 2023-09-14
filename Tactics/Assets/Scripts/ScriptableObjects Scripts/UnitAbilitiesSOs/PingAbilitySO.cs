using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        [SerializeField] private int range = 2;
        
        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return caster.Tile.IsInSurroundingTileDistance(selectableTile,range);
        }

        protected override void AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log($"Ping on {targetTiles[0]} and {targetTiles[1]}");
            
            EndAbility();
        }
    }
}