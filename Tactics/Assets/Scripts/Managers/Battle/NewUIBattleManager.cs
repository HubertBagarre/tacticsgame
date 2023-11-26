using System.Collections.Generic;
using Battle;
using Battle.InputEvent;
using Battle.UIComponent;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NewUIBattleManager : MonoBehaviour
{
    [Header("Unit UI")]
    [SerializeField] private UIUnit uiUnitPrefab;
    private Dictionary<NewUnit, UIUnit> uiUnitsDict = new ();
    
    [Header("Unit Tooltip")]
    [SerializeField] private UIUnitTooltip unitTooltip;
    
    [Header("Tooltip")]
    [SerializeField] private UITooltip tooltip;
    public static UITooltip Tooltip { get; private set; }
    
    [Header("Battle State")]
    [SerializeField] private RectTransform battleRoundIndicatorTr;
    [SerializeField] private TextMeshProUGUI battleRoundIndicatorText;
    [SerializeField] private RectTransform battleStartIndicatorTr;

    public void AddCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.invoked += InstantiateUnitUi;
        ActionStartInvoker<NewBattleManager.MainBattleAction>.invoked += PlayBattleStartAnimation;
        ActionStartInvoker<NewBattleManager.RoundAction>.invoked += PlayRoundStartAnimation;
        
        EventManager.AddListener<ClickUnitEvent>(ShowUnitTooltip);
    }
    
    public void RemoveCallbacks()
    {
        ActionEndInvoker<NewBattleManager.UnitCreatedAction>.invoked -= InstantiateUnitUi;
        ActionStartInvoker<NewBattleManager.MainBattleAction>.invoked -= PlayBattleStartAnimation;
        ActionStartInvoker<NewBattleManager.RoundAction>.invoked -= PlayRoundStartAnimation;
        
        EventManager.RemoveListener<ClickUnitEvent>(ShowUnitTooltip);
    }
    
    public void Start()
    {
        AssignTooltip();
        
        battleStartIndicatorTr.anchoredPosition = new Vector2(-Screen.width, battleStartIndicatorTr.anchoredPosition.y);
        battleRoundIndicatorTr.anchoredPosition = new Vector2(-Screen.width, battleRoundIndicatorTr.anchoredPosition.y);
        
        unitTooltip.SetupStatElement();
        unitTooltip.Hide();
        
        AddCallbacks();
    }
    
    private void InstantiateUnitUi(NewBattleManager.UnitCreatedAction action)
    {
        var unit = action.Unit;
        if(uiUnitsDict.ContainsKey(unit)) uiUnitsDict[unit].gameObject.SetActive(false);
        var ui = Instantiate(uiUnitPrefab, action.Renderer.UiParent);
        ui.LinkToUnit(unit);
            
        uiUnitsDict.Add(unit,ui);
    }
    
    private void ShowUnitTooltip(ClickUnitEvent ctx)
    {
        var unit = ctx.Unit;
        
        if (unit == null)
        {
            unitTooltip.Hide();
            return;
        }
            
        unitTooltip.DisplayUnitTooltip(ctx.Unit);
        unitTooltip.Show();
    }
    
    private void AssignTooltip()
    {
        Tooltip = tooltip;
        Tooltip.Hide();
    }
    
    private void PlayBattleStartAnimation(NewBattleManager.MainBattleAction action)
    {
        var posY = battleStartIndicatorTr.anchoredPosition.y;
            
        var sequence = DOTween.Sequence();
        sequence.Append(battleStartIndicatorTr.DOMoveX(0, action.BattleStartTransitionDuration.x));
        sequence.AppendInterval(action.BattleStartTransitionDuration.y);
        sequence.Append(battleStartIndicatorTr.DOMoveX(Screen.width, action.BattleStartTransitionDuration.z));
        sequence.AppendCallback(()=>battleStartIndicatorTr.anchoredPosition = new Vector2(-Screen.width,posY));

        sequence.Play();
    }
    
    private void PlayRoundStartAnimation(NewBattleManager.RoundAction action)
    {
        battleRoundIndicatorText.text = $"Round {action.CurrentRound}";

        var sequence = DOTween.Sequence();
        sequence.Append(battleRoundIndicatorTr.DOMoveX(0,action.TransitionDuration.x));
        sequence.AppendInterval(action.TransitionDuration.y);
        sequence.Append(battleRoundIndicatorTr.DOMoveX(Screen.width,action.TransitionDuration.z));
        sequence.AppendCallback(()=>battleRoundIndicatorTr.anchoredPosition = new Vector2(-Screen.width,0));

        sequence.Play();
    }
}
