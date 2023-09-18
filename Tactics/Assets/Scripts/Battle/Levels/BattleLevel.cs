using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using BattleEvents;
    
    public class BattleLevel : MonoBehaviour
    {
        public List<BattleEntity> StartingEntities => GetStartingEntities();

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

        protected virtual List<BattleEntity> GetStartingEntities()
        {
            return Units.Cast<BattleEntity>().ToList();
        }
    }
}