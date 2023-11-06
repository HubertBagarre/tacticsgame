using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public abstract class UnitAbilityRequirementSO : ScriptableObject
    {
        public virtual bool ConvertDescriptionLinks(Tile tile,string linkKey,out string text)
        {
            text = string.Empty;
            return false;
        }

        public abstract List<(string verb,string content)> Descriptions(Tile tile);
        
        public abstract bool CanCastAbility(Tile tile);

        public virtual void ConsumeRequirement(Tile tile)
        {
            
        }
    }
}


