using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Passive/NoEffect")]
    public class NoEffectUnitPassiveSo : UnitPassiveSo
    {
        protected override void OnAddedEffect(IPassivesContainer container, PassiveInstance instance)
        {
            
        }

        protected override void OnRemovedEffect(IPassivesContainer container, PassiveInstance instance)
        {
            
        }
    }
}


