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
    }

    public Tile GetClickTile()
    {
        InputManager.CastCamRay(out var tileHit, worldLayers);

        return tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
    }
    
    public void ClearSelectableTiles()
    {
        SelectableTiles.Clear();
        
        foreach (var tile in tiles)
        {
            tile.SetAppearance(Tile.Appearance.Default);
            tile.SetPathRing(0);
        }
        
    }

    public void SetSelectableTilesForMovement(Tile origin,int range,bool includeDiag,Func<Tile,bool> extraCondition = null)
    {
        ClearSelectableTiles();

        extraCondition ??= _ => true;

        SelectableTiles.Add(origin);
        var justAdded = new List<Tile>() {origin};
        var iteration = 0;
        
        origin.SetPathRing(iteration);
        
        AddNeighbors();
        
        foreach (var tile in SelectableTiles)
        {
            tile.SetAppearance(Tile.Appearance.Selectable);
        }
        
        void AddNeighbors()
        {
            if(iteration >= range) return;
            
            var neighbors = justAdded.SelectMany(tile => tile.GetDirectNeighbors(includeDiag)).Distinct().ToList();

            var validTiles = neighbors
                .Where(tile => tile != null)
                .Where(tile => !SelectableTiles.Contains(tile))
                .Where(extraCondition)
                .ToList();

            SelectableTiles.AddRange(validTiles);
            justAdded = validTiles;
            
            iteration++;
            
            foreach (var addedTile in justAdded)
            {
                addedTile.SetPathRing(iteration);
            }
            
            AddNeighbors();
        }
    }

    public List<Tile> GetPathFromSelectableTiles(Tile destination)
    {
        if (!SelectableTiles.Contains(destination)) return null;

        var path = new List<Tile>() {destination};
        var lastAdded = destination;
        
        for (int i = destination.PathRing - 1; i >= 1; i--)
        {
            lastAdded = lastAdded.GetDirectNeighbors().FirstOrDefault(tile => tile.PathRing == i);
            if(lastAdded == null) break;
            path.Add(lastAdded);
        }

        path.Reverse();
        
        return path;
    }
}