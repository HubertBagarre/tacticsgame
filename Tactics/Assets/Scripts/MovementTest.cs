using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private TileManager tileManager;

    private Unit unitToMove;

    private void Start()
    {
        InputManager.RightClickEvent += SelectUnit;
        InputManager.LeftClickEvent += MoveUnit;
    }

    private void SelectUnit()
    {
        unitToMove = unitManager.GetClickUnit();
        
        if(unitToMove == null) return;
        if(unitToMove.Tile == null) return;
        
        tileManager.SetSelectableTilesForMovement(unitToMove.Tile,unitToMove.Movement,false,unitToMove.Stats.WalkableTileSelector);
    }

    private void DeselectTiles()
    {
        Debug.Log("Clearing Selectables");
        
        tileManager.ClearSelectableTiles();
    }
    
    private void MoveUnit()
    {
        var destination = tileManager.GetClickTile();
        
        if(!tileManager.SelectableTiles.Contains(destination)) return;

        var path = tileManager.GetPathFromSelectableTiles(destination);
        
        DeselectTiles();

        unitManager.MoveUnit(unitToMove,path);
    }
    
}
