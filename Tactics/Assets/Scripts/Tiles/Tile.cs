using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Data Container for Tile Info
/// (tile position, adjacent Tiles, current Unit)
/// </summary>
public class Tile : MonoBehaviour
{
    //identification
    [SerializeField] private Vector2Int position;
    public Vector2Int Position => position;
    
    [SerializeField] private Unit currentUnit;
    
    //pathing
    [field: SerializeField] public bool IsWalkable { get; private set; }
    [SerializeField] private Tile[] neighbors; //0 is top (x,y+1), then clockwise, adjacent before diag
    [field:SerializeField] public int PathRing { get; private set; }

    //viusal
    [Header("Visual")] [SerializeField] private Renderer modelRenderer;

    [SerializeField] private Material defaultMat;
    [SerializeField] private Material selectableMat;
    [SerializeField] private Material selectedMat;
    [SerializeField] private Material unselectableMat;
    
    [field:Header("Debug")]
    [field:SerializeField] public TextMeshProUGUI DebugText { get; private set; }

    public enum Appearance
    {
        Default,
        Selectable,
        Selected,
        Unselectable,
    }

    public void InitPosition(int x, int y)
    {
        position = new Vector2Int(x, y);
    }

    public void InitNeighbors(Tile[] tiles)
    {
        neighbors = tiles;
    }

    public void SetWalkable(bool value)
    {
        IsWalkable = value;
    }

    public void RemoveUnit()
    {
        currentUnit = null;
    }

    public void SetUnit(Unit unit)
    {
        currentUnit = unit;
    }

    public bool HasUnit()
    {
        return currentUnit != null;
    }

    public Unit GetUnit()
    {
        return currentUnit;
    }

    public Tile[] GetDirectNeighbors(bool includeDiag = false)
    {
        var tiles = includeDiag ? neighbors : new[] {neighbors[0], neighbors[1], neighbors[2], neighbors[3]};
        
        return tiles.Where(tile => tile != null).ToArray();
    }

    public Tile[] GetNeighbors(int range, bool includeDiag = false)
    {
        //TODO Implement GetNeighbors method

        return range switch
        {
            <= 0 => new[] {this},
            1 => GetDirectNeighbors(),
            _ => GetDirectNeighbors()
        };
    }


    public void SetAppearance(Appearance appearance)
    {
        Material mat = appearance switch
        {
            Appearance.Default => defaultMat,
            Appearance.Selectable => selectableMat,
            Appearance.Selected => selectedMat,
            Appearance.Unselectable => unselectableMat,
            _ => defaultMat
        };

        modelRenderer.material = mat;
    }

    public void SetPathRing(int value)
    {
        PathRing = value;
        DebugText.text = $"{PathRing}";
    }
}