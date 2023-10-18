using System.Collections;
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

            var hasPath = caster.Tile.GetPath(destination,out var path,false,caster.Stats.Behaviour.WalkableTileSelector);

            if (hasPath)
            {
                caster.Tile.SetLineRendererPath(path);
                caster.Tile.ShowLineRendererPath();
            }
            
            yield return caster.StartCoroutine(caster.MoveUnit(path,forced));
        }
    }
}