using System.Collections;
using UnityEngine;

namespace Battle
{
    public class BattleStarter : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;

        [SerializeField] private BattleLevel defaultLevel;
    
        private IEnumerator Start() 
        {
            Debug.Log("Scene Loading, starting level");

            if(LevelManager.SelectedLevel == null) LevelManager.ChangeSelectedLevel(defaultLevel);
            var level = Instantiate(LevelManager.SelectedLevel);
            level.SetupCallbacks();
            
            Debug.Log("Setting up level");
            yield return StartCoroutine(battleManager.SetupBattle(level));
            Debug.Log("Done Setting up level");
            
            yield return null;
            
            Debug.Log("Starting Battle");
            battleManager.StartBattle();
        }
    }
}

