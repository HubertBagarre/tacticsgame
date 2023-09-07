using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Tile[] neighbors; //0 is top, then clockwise

    [SerializeField] private Unit unit;

    public void SetNeighbors(Tile[] tiles)
    {
        neighbors = tiles;
    }

    public bool HasUnit()
    {
        return unit != null;
    }
    
    public Unit GetUnit()
    {
        return unit;
    }
    
    public Tile[] GetNeighbors(bool includeDiag = false)
    {
        return includeDiag ? neighbors : new[] {neighbors[0], neighbors[2], neighbors[4], neighbors[6]};
    }
}
