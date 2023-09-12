using Battle;
using Battle.UIEvents;
using Battle.UnitEvents;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitBehaviour/PlayerUnit")]
public class PlayerUnitBehaviourSO : UnitBehaviourSO
{
    public override void InitBehaviour(Unit unit)
    {
        EventManager.AddListener<EndUnitTurnEvent>(EndPlayerControl);
        
        void EndPlayerControl(EndUnitTurnEvent ctx)
        {
            if (ctx.Unit != unit) return;
            
            EventManager.Trigger(new EndPlayerControlEvent());
        }
    }

    public override void RunBehaviour(Unit unit)
    {
        tileM.UpdateAvailableUnitMovementTiles(unit);
        
        EventManager.Trigger(new StartPlayerControlEvent());
    }
    
    
}

namespace Battle.UIEvents
{
    public class StartPlayerControlEvent {}
    public class EndPlayerControlEvent {}
}
