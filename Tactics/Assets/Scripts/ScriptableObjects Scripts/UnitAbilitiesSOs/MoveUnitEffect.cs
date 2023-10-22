using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Move Unit")]
    public class MoveUnitEffect : UnitAbilityEffectSO
    {
        [SerializeField] private bool forced = false;
        
        public override string ConvertedDescription(Unit caster)
        {
            return "Move to tile.";
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            var tile = caster.Tile;
            
            var destination = targetTiles[0];
            
            if (tile.GetPath(destination,out var path,false,caster.Stats.Behaviour.WalkableTileSelector))
            {
                
                
                tile.SetLineRendererPath(path);
                tile.ShowLineRendererPath();
            }
            
            yield return caster.StartCoroutine(caster.MoveUnit(path,forced));
        }
    }
}