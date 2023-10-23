using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Effect
{
    [CreateAssetMenu(fileName = "EffectsAndEffectsOnTileBehind", menuName = "Battle Scriptables/Effect Composite/Effects And Effects On Tile Behind")]
    public class EffectsAndEffectsOnTileBehindSO : UnitAbilityEffectSO
    {
        [SerializeField] private List<UnitAbilityEffectSO> mainEffects;
        [SerializeField] private List<UnitAbilityEffectSO> behindTileEffects;
        
        public override string ConvertedDescription(Unit caster)
        {
            var effectsText = string.Empty;
            foreach (var effect in mainEffects)
            {
                var desc = effect.ConvertedDescription(caster);
                if(!desc.EndsWith("\n")) desc += "\n";
                effectsText += $"{desc}";
            }
            effectsText = effectsText.TrimEnd('\n');
            return effectsText;
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var abilityEffect in mainEffects)
            {
                yield return caster.StartCoroutine(abilityEffect.AbilityEffect(caster, targetTiles));
            }

            foreach (var tile in targetTiles)
            {
                var behindTile = tile.GetNeighbor(caster.Tile.GetTileDirection(tile));
                if (behindTile == null) continue;
                foreach (var abilityEffect in behindTileEffects)
                {
                    yield return caster.StartCoroutine(abilityEffect.AbilityEffect(caster, new []{behindTile}));
                }
            }
        }
    }

}

