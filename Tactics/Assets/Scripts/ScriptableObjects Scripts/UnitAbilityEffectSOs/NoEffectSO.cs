using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/No Effect")]
    public class NoEffectSO : UnitAbilityEffectSO
    {
        [SerializeField] private string text = "Does nothing";
        
        public override string ConvertedDescription(Unit caster)
        {
            return text;
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            yield break;
        }
    }
}

