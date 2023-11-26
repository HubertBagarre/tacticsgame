using System.Collections;
using UnityEngine;

namespace Battle
{
    public class BattleStarter : MonoBehaviour
    {
        [SerializeField] private NewBattleManager newBattleManager;

        [SerializeField] private BattleLevel defaultLevel;
    
        private IEnumerator Start() 
        {
            Debug.Log("Scene Loading, starting level");

            yield return null;
            
            if(LevelManager.SelectedLevel == null) LevelManager.ChangeSelectedLevel(defaultLevel);
            var level = Instantiate(LevelManager.SelectedLevel);
            
            newBattleManager.SetLevel(level);
            newBattleManager.StartBattle();
            
            /*
            Debug.Log("Setting up level");
            yield return StartCoroutine(battleManager.SetupBattle(level));
            Debug.Log("Done Setting up level");
            
            yield return null;
            
            Debug.Log("Starting Battle");
            battleManager.StartBattle();
            */
        }
    }
}

