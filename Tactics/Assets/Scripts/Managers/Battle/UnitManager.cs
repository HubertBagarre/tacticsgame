using System.Collections.Generic;
using System.Linq;
using Battle.InputEvent;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Handles Unit Management
    ///
    /// Lists all units
    /// Unit movement 
    /// Unit abilities
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] protected LayerMask entityLayers;

        [SerializeField] private UnitRenderer defaultRendererPrefab;
        
        [Header("Debug")] [SerializeField] private List<Unit> units = new List<Unit>();
        
        public List<Unit> AllUnits => units.ToList();
        
        public void AddCallbacks()
        {
            InputManager.RightClickEvent += ClickUnit;
        }

        public void RemoveCallbacks()
        {
            InputManager.RightClickEvent -= ClickUnit;
        }
        
        public UnitRenderer SpawnUnit(NewUnit unit,UnitRenderer unitRendererPrefab,NewTile.Direction direction)
        {
            var useDefault = unitRendererPrefab == null;
            
            var unitRenderer = Instantiate(useDefault ? defaultRendererPrefab : unitRendererPrefab);
            unitRenderer.SetUnit(unit);
            
            unitRenderer.name = unit.Name;

            return unitRenderer;
        }

        public void SetUnits(List<Unit> list)
        {
            units = list;
        }
        
        private void ClickUnit()
        {
            EventManager.Trigger(new ClickUnitEvent(GetClickUnit()));
        }

        public NewUnit GetClickUnit()
        {
            InputManager.CastCamRay(out var unitHit, entityLayers);
            
            if(unitHit.transform == null) return null;

            if(!unitHit.transform.TryGetComponent(out UnitRenderer unitRenderer)) return null;
            
            return unitRenderer.Unit;
        }
    }
}

namespace Battle.InputEvent
{
    public class ClickUnitEvent
    {
        public NewUnit Unit { get; }

        public ClickUnitEvent(NewUnit unit)
        {
            Unit = unit;
        }
    }
}