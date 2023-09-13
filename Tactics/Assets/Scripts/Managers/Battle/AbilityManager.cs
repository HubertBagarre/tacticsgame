using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public class AbilityManager : MonoBehaviour
    {
        [SerializeField] private TileManager tileManager;
    }
}



namespace Battle.AbilityEvent
{
    public class StartAbilitySelectionEvent
    {
        
    }

    public class EndAbilitySelectionEvent
    {
        public bool Canceled { get; }

        public EndAbilitySelectionEvent(bool canceled)
        {
            Canceled = canceled;
        }
    }
}
