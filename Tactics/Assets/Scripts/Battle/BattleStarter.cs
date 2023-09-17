using System.Collections;
using UnityEngine;

namespace Battle
{
    public class BattleStarter : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TileGenerator levelGenerator;
    
        private IEnumerator Start()
        {
            Debug.Log("Scene Loading, starting level");
        
            levelGenerator.GenerateLevel();
            
            battleManager.SetupBattle(LevelManager.SelectedLevel);

            yield return null;
            
            battleManager.StartBattle();
        }
    }
}

