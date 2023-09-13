using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    
    public class LevelManager : MonoBehaviour
    {
        [field: SerializeField] private TileManager tileManager;
        [field: SerializeField] private UnitManager unitManager;
        [field: SerializeField] private TileGenerator generator;

        private void Start()
        {
            Setup();
            
            Run();
        }

        public void Setup()
        {
            generator.GenerateLevel();
            
            foreach (var tile in tileManager.AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
            }
            
            UnitBehaviourSO.SetTileManager(tileManager);
            UnitBehaviourSO.SetUnitManager(unitManager);
        }

        public void Run()
        {
            EventManager.Trigger(new StartLevelEvent(unitManager.AllUnits.Cast<BattleEntity>().ToList()));
        }
    }
}

public class SetupLevelEvent
{
    
}

public class StartLevelEvent
{
    public List<BattleEntity> StartingEntities { get; }

    public StartLevelEvent(List<BattleEntity> startingEntities)
    {
        StartingEntities = startingEntities;
    }
}


