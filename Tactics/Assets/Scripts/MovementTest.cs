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
    }
    
    private void MoveUnit()
    {
        unitManager.MoveUnit(unitToMove,tileManager.GetClickTile());
    }
    
}
