using UnityEngine;

namespace Battle
{
    public class LevelManager : MonoBehaviour
    {
        [field: SerializeField] private TileManager tileManager;
        [field: SerializeField] private UnitManager unitManager;

        private void Start()
        {
            Run();
        }

        public void Run()
        {
            foreach (var tile in tileManager.AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
            }
        
            EventManager.Trigger(new StartLevelEvent());
        }
    }
}

public class StartLevelEvent
{
    
}


