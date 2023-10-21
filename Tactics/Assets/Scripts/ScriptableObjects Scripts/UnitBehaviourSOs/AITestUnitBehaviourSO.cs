using System.Collections;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{

    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/Test")]
    public class AITestUnitBehaviourSO : UnitBehaviourSO
    {
        protected override void InitBehaviourEffect(Unit unit)
        {

        }

        public override void ShowBehaviourPreview()
        {

        }

        protected override IEnumerator RunBehaviourEffect()
        {
            var path = AssociatedUnit.Tile.GetAdjacentTiles().ToList();
            
            AssociatedUnit.SetMovement(path.Count);
            
            yield return AssociatedUnit.StartCoroutine(AssociatedUnit.MoveUnit(path,false));
        }

        protected override void OnBehaviourInterruptedEffect()
        {
            
        }
    }
}