using System.Collections.Generic;
using System.Linq;
using Battle.ScriptableObjects;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using BattleEvents;
    
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private UnitPlacementSO unitPlacement;
        public List<IBattleEntity> StartingEntities => SetupStartingEntities();
        
        [field:SerializeField] public List<Tile> Tiles { get; private set; }
        [field:SerializeField] public List<Unit> Units { get; private set; }

        public virtual void SetupCallbacks()
        {
            
        }

        public virtual void SetupEndBattleConditions(BattleManager battleManager)
        {
            EventManager.AddListener<UnitDeathEvent>(TryEndBattle);
            
            void TryEndBattle(UnitDeathEvent ctx)
            {
                var entities = battleManager.EntitiesInBattle.Where(entity => !entity.IsDead).ToArray();
                if (entities.Length <= 0)
                {
                    Debug.Log("No entities, lose");
                    battleManager.LoseBattle();
                    return;
                }

                if (entities.All(entity => entity.Team == 0))
                {
                    Debug.Log("All allies, win");
                    battleManager.WinBattle();
                    return;
                }
                
                if (entities.All(entity => entity.Team != 0))
                {
                    Debug.Log("No allies, lose");
                    battleManager.LoseBattle();
                    return;
                }
                
                Debug.Log("End Conditions not Met");
            }
        }

        public void SetTiles(List<Tile> tiles)
        {
            Tiles = tiles;
        }

        public void SetUnits(List<Unit> units)
        {
            Units = units;
        }
        
        protected virtual List<IBattleEntity> SetupStartingEntities()
        {
            var units = new List<Unit>();
            foreach (var placedUnit in unitPlacement.PlacedUnits)
            {
                var tile = Tiles.FirstOrDefault(tile => tile.Position == placedUnit.position);
                
                var unit = Instantiate(placedUnit.prefab);
                var unitTr = unit.transform;
                unitTr.position = tile.transform.position;
                unitTr.rotation = Quaternion.identity;
                unitTr.SetParent(transform);

                unit.name = placedUnit.so.name;
                unit.InitUnit(tile,placedUnit.team,placedUnit.so,placedUnit.orientation);
            
                units.Add(unit);
            }

            SetUnits(units);
            
            return Units.Cast<IBattleEntity>().ToList();
        }
    }
}