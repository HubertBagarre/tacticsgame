using System;
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
    [Header("Settings")] [SerializeField] protected LayerMask worldLayers;

    [Header("Debug")] [SerializeField] private List<Tile> tiles = new List<Tile>();

    public List<Tile> AllTiles => tiles.ToList();

    public List<Tile> SelectableTiles { get; private set; } = new List<Tile>();

    public void SetTiles(List<Tile> list)
    {
        tiles = list;
        Debug.Log($"Set {tiles.Count} tiles");
    }

    public Tile GetClickTile()
    {
        InputManager.CastCamRay(out var tileHit, worldLayers);

        return tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
    }
    
    public void ClearSelectableTiles()
    {
        foreach (var tile in SelectableTiles)
        {
            tile.SetAppearance(Tile.Appearance.Default);
        }
        SelectableTiles.Clear();
    }

    public void SetSelectableTiles(Tile origin,int range,bool includeDiag,Func<Tile,bool> extraCondition = null)
    {
        Debug.Log($"Setting Selectable Tiles (range = {range})");

        ClearSelectableTiles();

        extraCondition ??= _ => true;

        SelectableTiles.Add(origin);
        var justAdded = new List<Tile>() {origin};
        var iteration = 0;
        
        AddNeighbors();
        
        foreach (var tile in SelectableTiles)
        {
            tile.SetAppearance(Tile.Appearance.Selectable);
        }
        
        void AddNeighbors()
        {
            if(iteration >= range) return;
            
            var neighbors = justAdded.SelectMany(tile => tile.GetNeighbors(includeDiag)).ToList();

            var validTiles = neighbors
                .Where(tile => tile != null)
                .Where(tile => !SelectableTiles.Contains(tile))
                .Where(extraCondition)
                .ToList();
            
            
            SelectableTiles.AddRange(validTiles);
            justAdded = validTiles;
            
            iteration++;
            
            AddNeighbors();
        }
    }
}