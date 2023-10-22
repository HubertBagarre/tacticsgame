using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class UnitAbilityRequirementSO : ScriptableObject
    {
        public virtual bool ConvertDescriptionLinks(Unit caster,string linkKey,out string text)
        {
            text = string.Empty;
            return false;
        }

        public abstract List<(string verb,string content)> Descriptions(Unit caster);
        
        public abstract bool CanCastAbility(Unit caster);

        public virtual IEnumerator ConsumeRequirement(Unit caster)
        {
            yield break;
        }
    }
}


