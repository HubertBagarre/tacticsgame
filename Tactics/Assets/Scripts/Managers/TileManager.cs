using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles Tiles
/// </summary>
public class TileManager : MonoBehaviour
{
    [SerializeField] private List<Tile> tiles = new List<Tile>();
    [SerializeField] private List<Unit> units = new List<Unit>();

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
}
