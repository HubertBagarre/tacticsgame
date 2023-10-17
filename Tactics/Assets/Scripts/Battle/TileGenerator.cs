using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Battle;
using Battle.ScriptableObjects;
using UnityEditor;

//TODO - Change to level generator
/// <summary>
/// Generates level
/// = instantiate tiles, link neighbors
/// = instantiate units
/// </summary>
public class TileGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleLevel battleLevel;
    [SerializeField] private Transform transformParent;
    
    [Header("Tile Parameters")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 tileSpacing;
    
    [Header("Lists")]
    [SerializeField] private List<Tile> tiles;
    [SerializeField] private List<Unit> units;
    private Tile[,] grid;
    
#if UNITY_EDITOR
    
    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        ClearList(tiles);
        ClearList(units);
        
        tiles = new List<Tile>();
        units = new List<Unit>();
        
        grid = new Tile[gridSize.x,gridSize.y];
        
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {

                var pos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y);
                var tile = PrefabUtility.InstantiatePrefab(tilePrefab) as Tile;
                var tileTr = tile.transform;
                tileTr.position = pos;
                tileTr.rotation = Quaternion.identity;
                tileTr.SetParent(transformParent);
                
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
            if(!right) neighbors[1] = grid[x + 1,y];
            if(!bot) neighbors[2] = grid[x,y - 1];
            if(!left) neighbors[3] = grid[x - 1,y];
            if(!right && !top) neighbors[4] = grid[x + 1,y + 1];
            if(!right && !bot) neighbors[5] = grid[x + 1,y - 1];
            if(!left && !bot) neighbors[6] = grid[x - 1,y - 1];
            if(!left && !top) neighbors[7] = grid[x - 1,y + 1];
            
            tile.InitNeighbors(neighbors);
        }

        battleLevel.SetTiles(tiles);

        foreach (var tile in tiles)
        {
            tile.SetWalkable(true);
        }
        
        EditorUtility.SetDirty(transformParent);
    }

    private void ClearList<T>(List<T> list) where T : MonoBehaviour
    {
        if (list == null) return;
        if (list.Count <= 0)
        {
            list.Clear();
            return;
        }

        int count = list.Count;
        for (int i = 0; i < count; i++)
        {

            DestroyImmediate(list[0].gameObject);

            list.RemoveAt(0);
        }

        list.Clear();
    }
#endif
}