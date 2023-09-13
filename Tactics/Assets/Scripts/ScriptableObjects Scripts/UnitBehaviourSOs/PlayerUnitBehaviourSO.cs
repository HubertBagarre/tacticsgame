using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using UIEvents;
    using UnitEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/PlayerUnit")]
    public class PlayerUnitBehaviourSO : UnitBehaviourSO
    {
        private List<Tile> selectableTilesForMovement = new List<Tile>();
        private Unit controlledUnit;

        public override void InitBehaviour(Unit unit)
        {
            controlledUnit = unit;

            EventManager.AddListener<EndUnitTurnEvent>(EndPlayerControl);
            
            void EndPlayerControl(EndUnitTurnEvent ctx)
            {
                if (ctx.Unit != unit) return;
                
                EventManager.Trigger(new EndPlayerControlEvent());
            }
        }

        public override void ShowBehaviourPreview(Unit unit)
        {
        }

        public override void RunBehaviour(Unit unit)
        {
            EventManager.Trigger(new StartPlayerControlEvent(unit));
        }
    }
}


namespace Battle.UIEvents
{
    public class StartPlayerControlEvent
    {
        public Unit PlayerUnit { get; }

        public StartPlayerControlEvent(Unit playerUnit)
        {
            PlayerUnit = playerUnit;
        }
    }

    public class EndPlayerControlEvent
    {
    }

    public class StartUnitMovementSelectionEvent
    {
        public Unit Unit { get; }
        public List<Tile> SelectableTiles { get; }

        public StartUnitMovementSelectionEvent(Unit unit,List<Tile> selectableTiles)
        {
            Unit = unit;
            SelectableTiles = selectableTiles;
        }
    }

    public class EndUnitMovementSelectionEvent
    {
        
    }
}