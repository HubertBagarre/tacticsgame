using System;
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

        [Header("Debug")] [SerializeField] private List<Unit> units = new List<Unit>();
        public List<Unit> AllUnits => units.ToList();

        public void Start()
        {
            InputManager.RightClickEvent += ClickUnit;
        }

        public void SetUnits(List<Unit> list)
        {
            units = list;
        }
        
        private void ClickUnit()
        {
            EventManager.Trigger(new ClickUnitEvent(GetClickUnit()));
        }

        public Unit GetClickUnit()
        {
            InputManager.CastCamRay(out var unitHit, entityLayers);

            return unitHit.transform != null ? unitHit.transform.GetComponent<Unit>() : null;
        }
    }
}

namespace Battle.InputEvent
{
    public class ClickUnitEvent
    {
        public Unit Unit { get; }

        public ClickUnitEvent(Unit unit)
        {
            Unit = unit;
        }
    }
}