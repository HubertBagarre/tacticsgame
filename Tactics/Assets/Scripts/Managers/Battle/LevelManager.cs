using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battle
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private int levelSceneIndex;
        
        [Header("UI")]
        [SerializeField] private Button startLevelButton;

        public static BattleLevel SelectedLevel{ get; private set; }

        private void Start()
        {
            startLevelButton.onClick.AddListener(GoToLevelScene);
        }

        public static void ChangeSelectedLevel(BattleLevel level)
        {
            SelectedLevel = level;
        }

        private void GoToLevelScene()
        {
            Debug.Log("Launching level");
            
            // TODO - Loading screen here
            
            SceneManager.LoadScene(levelSceneIndex);
        }
    }
}
