using Battle;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerControls : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private GameObject parent;

    [SerializeField] private UIAbilityManager uiAbilityManager;

    [Header("Player Controls")] [SerializeField]
    private Button endTurnButton;

    private UnitTurnBattleAction currentTurnUnitAction;

    private void Start()
    {
        HidePlayerUI();

        AddCallbacks();
    }

    private void AddCallbacks()
    {
        ActionStartInvoker<UnitTurnBattleAction>.OnInvoked += TryShowPlayerUI;
        ActionEndInvoker<UnitTurnBattleAction>.OnInvoked += HidePlayerUI;
        
        EventManager.AddListener<StartAbilityTargetSelectionEvent>(ShowAbilityTargetSelection);
        EventManager.AddListener<EndAbilityTargetSelectionEvent>(HideAbilityTargetSelection);
    }

    private void RemoveCallbacks()
    {
        ActionStartInvoker<UnitTurnBattleAction>.OnInvoked -= TryShowPlayerUI;
        ActionEndInvoker<UnitTurnBattleAction>.OnInvoked -= HidePlayerUI;
        
        EventManager.RemoveListener<StartAbilityTargetSelectionEvent>(ShowAbilityTargetSelection);
        EventManager.RemoveListener<EndAbilityTargetSelectionEvent>(HideAbilityTargetSelection);
    }

    private void TryShowPlayerUI(UnitTurnBattleAction action)
    {
        currentTurnUnitAction = action;

        if (!currentTurnUnitAction.IsPlayerTurn) return;

        ShowPlayerUI();
    }

    private void ShowPlayerUI()
    {
        parent.SetActive(true);

        uiAbilityManager.ShowUnitAbilitiesButton(currentTurnUnitAction.Unit);

        endTurnButton.onClick.AddListener(currentTurnUnitAction.RequestEndTurn);
    }

    private void HidePlayerUI(UnitTurnBattleAction _ = null)
    {
        if (currentTurnUnitAction != null)
        {
            endTurnButton.onClick.RemoveListener(currentTurnUnitAction.RequestEndTurn);
        }

        currentTurnUnitAction = null;
        
        uiAbilityManager.ShowAbilityTargetSelection(false);
        uiAbilityManager.HideUnitAbilitiesButton();

        parent.SetActive(false);
    }

    private void ShowAbilityTargetSelection(StartAbilityTargetSelectionEvent ctx)
    {
        uiAbilityManager.ShowAbilityTargetSelection();
    }

    private void HideAbilityTargetSelection(EndAbilityTargetSelectionEvent ctx)
    {
        uiAbilityManager.ShowAbilityTargetSelection(false);
    }
}