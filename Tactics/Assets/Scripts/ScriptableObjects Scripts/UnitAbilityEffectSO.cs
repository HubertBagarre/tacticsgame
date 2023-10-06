using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{

    public abstract class UnitAbilityEffectSO : ScriptableObject
    {
        public virtual bool ConvertDescriptionLinks(Unit caster,string linkKey,out string text)
        {
            text = string.Empty;
            return false;
        }

        public abstract string ConvertedDescription(Unit caster);
        
        public abstract IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles);
    }
}