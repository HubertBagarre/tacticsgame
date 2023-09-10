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
    public List<Unit> AllUnits => units.ToList();

    public void SetUnits(List<Unit> list)
    {
        units = list;
    }

    public Unit GetClickUnit()
    {
        InputManager.CastCamRay(out var unitHit, entityLayers);

        return unitHit.transform != null ? unitHit.transform.GetComponent<Unit>() : null;;
    }

    public void MoveUnit(Unit unit,List<Tile> path)
    {
        Debug.Log($"Moving unit {unit} to {path.LastOrDefault()}");
        
        if(unit == null) return; //does the unit exist ?
        if(!path.Any()) return; // checks for valid path
        if(path.Any(tile => tile.HasUnit())) return; //does the path have any unit on it ?
        
        if(unit.Tile != null) unit.Tile.RemoveUnit();

        StartCoroutine(MoveAnimationRoutine());
        
        IEnumerator MoveAnimationRoutine()
        {
            foreach (var tile in path)
            {
                yield return null;

                unit.transform.position = tile.transform.position;
                
                tile.SetUnit(unit);
                unit.SetTile(tile);
            }
        }
    }
}