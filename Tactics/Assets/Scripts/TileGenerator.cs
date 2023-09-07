using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 tileSpacing;
    [SerializeField] private List<Tile> tiles;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        ClearTiles();
        tiles = new List<Tile>();
        for (int y = 0; y < gridSize.y; y++)
        {
             for (int x = 0; x < gridSize.x; x++) 
             {
                var pos = new Vector3(x * tileSpacing.x, 0, y * tileSpacing.y);
                var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile {x},{y}";
                tiles.Add(tile); 
             }
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