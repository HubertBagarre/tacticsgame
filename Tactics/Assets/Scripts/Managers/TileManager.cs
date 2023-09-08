using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles Tiles
/// </summary>
public class TileManager : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private Camera cam;
    
    [Header("Settings")]
    [SerializeField] protected LayerMask entityLayers;
    [SerializeField] protected LayerMask worldLayers;
    
    [Header("Debug")]
    [SerializeField] private List<Tile> tiles = new List<Tile>();
    [SerializeField] private List<Unit> units = new List<Unit>();
    
    [field:SerializeField] public Tile SelectedTile { get; private set; }
    [field:SerializeField] public Unit SelectedUnit { get; private set; }

    public List<Tile> AllTiles => tiles.ToList();
    public List<Unit> AllUnits => units.ToList();

    public void SetTiles(List<Tile> list)
    {
        tiles = list;
        Debug.Log($"Set {tiles.Count} tiles");
    }

    public void SetUnits(List<Unit> list)
    {
        units = list;
        Debug.Log($"Set {units.Count} units");
    }

    public void ConnectInputs()
    {
        InputManager.LeftClickEvent += UpdateTargets;
        InputManager.RightClickEvent += UpdateTargets;
    }
    
    private void UpdateTargets()
    {
        CastCamRay(out var unitHit, out var tileHit);
        
        if (tileHit.transform != null)
        {
            SelectedTile = tileHit.transform.GetComponent<Tile>();
        }
        else
        {
            SelectedTile = null;
        }
        
        if (unitHit.transform != null)
        {
            SelectedUnit = unitHit.transform.GetComponent<Unit>();
        }
        else
        {
            SelectedUnit = null;
        }
    }

    private void CastCamRay(out RaycastHit entityHit, out RaycastHit worldHit)
    {
        if (cam == null)
        {
            entityHit = new RaycastHit();
            worldHit = new RaycastHit();
            return;
        }

        var mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 100f);

        Physics.Raycast(mouseRay, out entityHit, Mathf.Infinity, entityLayers);
        Physics.Raycast(mouseRay, out worldHit, Mathf.Infinity, worldLayers);
    }
}
