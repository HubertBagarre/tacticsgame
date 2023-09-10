using System;
using System.Collections.Generic;
using UnityEngine;

//TODO - Change to level generator
/// <summary>
/// Generates level
/// = instantiate tiles, link neighbors
/// = instantiate units
/// </summary>
public class TileGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private UnitManager unitManager;
    
    [Header("Tile Parameters")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 tileSpacing;

    [Header("Unit Parameters")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private List<PlacedUnit> placedUnits;

    [Serializable]
    private class PlacedUnit
    {
        public UnitStatsSO so;
        public Vector2Int position;
        public int team;
    }
    
    [Header("Lists")]
    [SerializeField] private List<Tile> tiles;
    [SerializeField] private List<Unit> units;
    private Tile[,] grid;
    

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
            if(!right) neighbors[1] = grid[x + 1,y];
            if(!bot) neighbors[2] = grid[x,y - 1];
            if(!left) neighbors[3] = grid[x - 1,y];
            if(!right && !top) neighbors[4] = grid[x + 1,y + 1];
            if(!right && !bot) neighbors[5] = grid[x + 1,y - 1];
            if(!left && !bot) neighbors[6] = grid[x - 1,y - 1];
            if(!left && !top) neighbors[7] = grid[x - 1,y + 1];
            
            tile.InitNeighbors(neighbors);
        }

        tileManager.SetTiles(tiles);

        foreach (var tile in tiles)
        {
            tile.SetWalkable(true);
        }
        
        foreach (var placedUnit in placedUnits)
        {
            var tile = grid[placedUnit.position.x, placedUnit.position.y];
            var unit = Instantiate(unitPrefab,tile.transform.position,Quaternion.identity,transform);

            unit.name = placedUnit.so.name;
            unit.InitUnit(tile,placedUnit.team,placedUnit.so);
            
            units.Add(unit);
        }

        unitManager.SetUnits(units);
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
#if UNITY_EDITOR
            DestroyImmediate(list[0].gameObject);
#else
            Destroy(list[0]);
#endif
            list.RemoveAt(0);
        }

        list.Clear();
    }
}