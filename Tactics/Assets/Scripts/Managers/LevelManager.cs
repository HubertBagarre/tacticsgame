using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [field: SerializeField] private TileManager tileManager;
    [field: SerializeField] private UnitManager unitManager;

    private void Start()
    {
        InputManager.LeftClickEvent += DebugTile;
        InputManager.RightClickEvent += DebugUnit;

        Run();
        
        void DebugTile()
        {
            Debug.Log($"Clicked {tileManager.GetClickTile()}",this);
        }
        
        void DebugUnit()
        {
            Debug.Log($"Clicked {unitManager.GetClickUnit()}",this);
        }
    }

    public void Run()
    {
        foreach (var tile in tileManager.AllTiles)
        {
            tile.SetMat(Tile.Appearance.Unselected);
        }
    }
}
