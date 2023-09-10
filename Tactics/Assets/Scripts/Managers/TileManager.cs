using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles Tiles
///
/// List all tiles
/// </summary>
public class TileManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected LayerMask worldLayers;
    
    [Header("Debug")]
    [SerializeField] private List<Tile> tiles = new List<Tile>();
    
    [field:SerializeField] public Tile SelectedTile { get; private set; }
    
    public List<Tile> AllTiles => tiles.ToList();
    
    public void SetTiles(List<Tile> list)
    {
        tiles = list;
        Debug.Log($"Set {tiles.Count} tiles");
    }
    
    public Tile GetClickTile()
    {
        InputManager.CastCamRay(out var tileHit, worldLayers);
        
        SelectedTile = tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
        
        return SelectedTile;
    }
}
