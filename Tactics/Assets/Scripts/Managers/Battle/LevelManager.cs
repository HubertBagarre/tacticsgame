using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static Level SelectedLevel{ get; private set; } =  new Level();

        private void Start()
        {
            startLevelButton.onClick.AddListener(GoToLevelScene);
        }

        public void ChangeSelectedLevel(Level level)
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

public class Level
{
    public List<BattleEntity> StartingEntities;
}

