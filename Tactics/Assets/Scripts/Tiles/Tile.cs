using UnityEngine;

/// <summary>
/// Data Container for Tile Info
/// (tile position, adjacent Tiles, current Unit)
/// </summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private Vector2Int position;
    public Vector2Int Position => position;
    [SerializeField] private Tile[] neighbors; //0 is top, then clockwise

    [SerializeField] private Unit currentUnit;

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
}
