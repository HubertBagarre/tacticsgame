using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [field: SerializeField] public Unit SelectedUnit { get; private set; }

    public List<Unit> AllUnits => units.ToList();

    public void SetUnits(List<Unit> list)
    {
        units = list;
        Debug.Log($"Set {units.Count} units");
    }

    public Unit GetClickUnit()
    {
        InputManager.CastCamRay(out var unitHit, entityLayers);

        SelectedUnit = unitHit.transform != null ? unitHit.transform.GetComponent<Unit>() : null;

        return SelectedUnit;
    }

    public void MoveUnit(Unit unit,Tile destination)
    {
        Debug.Log($"Moving unit {unit} to {destination}");
        
        if(unit == null) return;
        if(destination == null) return;
        if(destination.HasUnit()) return;
        
        StartCoroutine(MoveAnimationRoutine());
        
        IEnumerator MoveAnimationRoutine()
        {
            yield return null;

            unit.transform.position = destination.transform.position;
        }
    }
}