using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Move Unit")]
    public class MoveUnitEffect : UnitAbilityEffectSO
    {
        [SerializeField] private bool forced = false;
        
        public override string ConvertedDescription(Unit caster)
        {
            return "Move the unit.";
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            var destination = targetTiles[0];


            var hasPath = caster.Tile.GetPath(destination,out var path);

            if (hasPath)
            {
                caster.Tile.SetPath(path);
                caster.Tile.ShowPath();
            }
            
            /*
            path = new List<Tile>() {targetTiles[0]};
            var lastAdded = destination;

            for (int i = destination.PathRing - 1; i >= 1; i--)
            {
                lastAdded = lastAdded.GetAdjacentTiles().FirstOrDefault(tile => tile.PathRing == i);
                if (lastAdded == null) break;
                path.Add(lastAdded);
            }

            path.Reverse();
            */

            yield return caster.StartCoroutine(caster.MoveUnit(path,forced));
        }
    }
}