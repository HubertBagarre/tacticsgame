using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data Container for Tile Info
/// (tile position, adjacent Tiles, current Unit)
/// </summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private Vector2Int position;
    public Vector2Int Position => position;
    [SerializeField] private Tile[] neighbors; //0 is top (x,y+1), then clockwise

    [SerializeField] private Unit currentUnit;

    [Header("Visual")] [SerializeField] private Renderer modelRenderer;

    [SerializeField] private Material defaultMat;
    [SerializeField] private Material selectableMat;
    [SerializeField] private Material selectedMat;
    [SerializeField] private Material unselectableMat;

    public enum Appearance
    {
        Unselected,
        Selectable,
        Selected,
        Unselectable,
    }

    public void InitPosition(int x,int y)
    {
        position = new Vector2Int(x,y);
    }
    
    public void InitNeighbors(Tile[] tiles)
    {
        neighbors = tiles;
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
    
    public Tile[] GetNeighbors(bool includeDiag = false)
    {
        return includeDiag ? neighbors : new[] {neighbors[0], neighbors[2], neighbors[4], neighbors[6]};
    }

    
    public void SetMat(Appearance appearance)
    {
        Material mat = appearance switch
        {
            Appearance.Unselected => defaultMat,
            Appearance.Selectable => selectableMat,
            Appearance.Selected => selectedMat,
            Appearance.Unselectable => unselectableMat,
            _ => defaultMat
        };

        modelRenderer.material = mat;
    }
}
