using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles Tiles
/// </summary>
public class TileManager : MonoBehaviour
{
    private List<Tile> tiles = new List<Tile>();
    private List<Unit> units = new List<Unit>();

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
}
