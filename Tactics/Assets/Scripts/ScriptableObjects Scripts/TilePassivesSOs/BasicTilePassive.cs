using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Tile Passive/Basic Tile Passive")]
    public class BasicTilePassive : TilePassiveSo<Tile>
    {
        protected override IEnumerator OnAddedEffect(Tile container, PassiveInstance<Tile> instance)
        {
            Debug.Log("Added Passive");
            yield break;
        }

        protected override IEnumerator OnRemovedEffect(Tile container, PassiveInstance<Tile> instance)
        {
            Debug.Log("Removed Passive");
            yield break;
        }

        protected override IEnumerator UnitEnterEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            Debug.Log("Unit entered tile");
            yield break;
        }

        protected override IEnumerator UnitExitEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            Debug.Log("Unit exited tile");
            yield break;
        }
    }
}


