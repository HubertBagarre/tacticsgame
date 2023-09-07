using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 tileSpacing;
    [SerializeField] private List<Tile> tiles;
    private Tile[,] grid;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        ClearTiles();
        tiles = new List<Tile>();
        grid = new Tile[gridSize.x,gridSize.y];
        
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {

                var pos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y);
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile {x},{y}";
                tile.InitPosition(x,y);
                tiles.Add(tile);
                grid[x,y] = tile;
            }
        }

        foreach (var tile in tiles)
        {
            var x = tile.Position.x;
            var y = tile.Position.y;
            var neighbors = new Tile[8];

            bool top = y == gridSize.y - 1;
            bool bot = y == 0;
            bool right = x == gridSize.x - 1;
            bool left = x == 0;
            
            if(!top) neighbors[0] = grid[x,y + 1];
            if(!right && !top) neighbors[1] = grid[x + 1,y + 1];
            if(!right) neighbors[2] = grid[x + 1,y];
            if(!right && !bot) neighbors[3] = grid[x + 1,y - 1];
            if(!bot) neighbors[4] = grid[x,y - 1];
            if(!left && !bot) neighbors[5] = grid[x - 1,y - 1];
            if(!left) neighbors[6] = grid[x - 1,y];
            if(!left && !top) neighbors[7] = grid[x - 1,y + 1];
            
            tile.InitNeighbors(neighbors);
        }
    }
    
    private void ClearTiles()
    {
        if (tiles == null) return;
        if (tiles.Count <= 0)
        {
            tiles.Clear();
            return;
        }

        int count = tiles.Count;
        for (int i = 0; i < count; i++)
        {
#if UNITY_EDITOR
            DestroyImmediate(tiles[0].gameObject);
#else
            Destroy(tiles[0]);
#endif
            tiles.RemoveAt(0);
        }

        tiles.Clear();
    }
}