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

        public override void RunBehaviour(Unit unit)
        {
            Debug.Log("AI Behaviour");

            EventManager.AddListener<UnitMovementEndEvent>(EndTurnOnMovementEnd, true);

            unit.MoveUnit(unit.Tile.GetAdjacentTiles().ToList());

            void EndTurnOnMovementEnd(UnitMovementEndEvent ctx)
            {
                if (ctx.Unit != unit) return;

                battleM.EndCurrentEntityTurn();
            }
        }
    }
}