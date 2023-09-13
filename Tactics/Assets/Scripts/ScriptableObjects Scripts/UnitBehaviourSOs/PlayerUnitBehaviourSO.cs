using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using UIEvents;
    using UnitEvents;
    using AbilityEvent;
    
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
            EventManager.AddListener<EndAbilityCastEvent>(EndTurnAfterAbilityCast);
            EventManager.AddListener<EndUnitTurnEvent>(RemoveEndTurnListenerAtTurnEnd,true);
            
            EventManager.Trigger(new StartPlayerControlEvent(unit));
        }
        
        private void EndTurnAfterAbilityCast(EndAbilityCastEvent ctx)
        {
            if (!ctx.Ability.EndUnitTurnAfterCast) return;
            
            battleM.EndCurrentEntityTurn();
        }
        
        private void RemoveEndTurnListenerAtTurnEnd(EndUnitTurnEvent ctx)
        {
            EventManager.RemoveListener<EndAbilityCastEvent>(EndTurnAfterAbilityCast);
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