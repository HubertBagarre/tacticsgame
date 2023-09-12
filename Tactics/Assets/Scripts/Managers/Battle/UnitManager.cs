using System.Collections.Generic;
using System.Linq;
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

        public void SetUnits(List<Unit> list)
        {
            units = list;
        }

        public Unit GetClickUnit()
        {
            InputManager.CastCamRay(out var unitHit, entityLayers);

            return unitHit.transform != null ? unitHit.transform.GetComponent<Unit>() : null;
        }
    }
}