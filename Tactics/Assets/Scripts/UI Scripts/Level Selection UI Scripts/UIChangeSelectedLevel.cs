using Battle;
using UnityEngine;
using UnityEngine.UI;

public class UIChangeSelectedLevel : MonoBehaviour
{
    [SerializeField] private Button button;
    [Header("Settings")]
    [SerializeField] private BattleLevel associatedLevel;
    
    private void Start()
    {
        button.onClick.AddListener(ChangeSelectedLevel);

        void ChangeSelectedLevel()
        {
            LevelManager.ChangeSelectedLevel(associatedLevel);
        }
    }
}
