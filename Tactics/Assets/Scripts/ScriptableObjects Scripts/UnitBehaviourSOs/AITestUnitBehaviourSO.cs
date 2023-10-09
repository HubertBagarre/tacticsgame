using System.Collections;
using System.Linq;
using Battle.UnitEvents;
using UnityEngine;

namespace Battle.ScriptableObjects
{

    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/Test")]
    public class AITestUnitBehaviourSO : UnitBehaviourSO
    {
        public override void InitBehaviour(Unit unit)
        {

        }

        public override void ShowBehaviourPreview(Unit unit)
        {

        }

        public override IEnumerator RunBehaviour(Unit unit)
        {
            var path = unit.Tile.GetAdjacentTiles().ToList();
            
            unit.SetMovement(path.Count);
            
            yield return unit.StartCoroutine(unit.MoveUnit(path,false));
        }

        public override void OnBehaviourInterrupted(Unit unit)
        {
            
        }
    }
}