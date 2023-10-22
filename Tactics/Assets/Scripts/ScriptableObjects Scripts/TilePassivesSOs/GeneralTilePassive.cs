using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Tile Passive/Basic Tile Passive")]
    public class GeneralTilePassive : TilePassiveSo<Tile>
    {
        [SerializeField] private bool removeOnUnitExit = false;
        [SerializeField] private bool removeOnUnitEnter = false;
        [SerializeField] private int stacksModifierOnUnitEnter = 0;
        [SerializeField] private int stacksModifierOnUnitExit = 0;
        [SerializeField] private List<PassiveToAdd> passiveToAddOnUnitEnter = new List<PassiveToAdd>();
        
        protected override IEnumerator OnAddedEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            if(removeOnUnitEnter || passiveToAddOnUnitEnter.Count > 0) tile.AddOnUnitEnterEvent(UnitEnterGeneralEffect(tile, instance));
            if(removeOnUnitExit) tile.AddOnUnitExitEvent(UnitExitGeneralEffect(tile, instance));
            yield break;
        }

        protected override IEnumerator OnRemovedEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            RemoveEvents(tile,instance);
            yield break;
        }

        private IEnumerator UnitEnterGeneralEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            foreach (var passiveToAdd in passiveToAddOnUnitEnter)
            {
                yield return tile.StartCoroutine(passiveToAdd.AddPassive(tile));
            }
            
            if(stacksModifierOnUnitEnter != 0) yield return tile.StartCoroutine(instance.IncreaseStacks(stacksModifierOnUnitEnter));
            
            if(removeOnUnitEnter)
            {
                RemoveEvents(tile,instance);
                yield return tile.StartCoroutine(tile.RemovePassiveEffect(instance));
            }
        }

        private IEnumerator UnitExitGeneralEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            if(stacksModifierOnUnitExit != 0) yield return tile.StartCoroutine(instance.IncreaseStacks(stacksModifierOnUnitExit));
            
            if (removeOnUnitExit)
            {
                RemoveEvents(tile,instance);
                yield return tile.StartCoroutine(tile.RemovePassiveEffect(instance));
            }
        }

        private void RemoveEvents(Tile tile, PassiveInstance<Tile> instance)
        {
            if(removeOnUnitEnter) tile.RemoveOnUnitEnterEvent(UnitEnterGeneralEffect(tile, instance));
            if(removeOnUnitExit) tile.RemoveOnUnitExitEvent(UnitExitGeneralEffect(tile, instance));
        }

        protected override IEnumerator UnitEnterEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            yield break;
        }

        protected override IEnumerator UnitExitEffect(Tile tile, PassiveInstance<Tile> instance)
        {
            yield break;
        }
    }
}


