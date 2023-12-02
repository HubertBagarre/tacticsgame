using UnityEngine;
using Battle;

public class UnitRenderer : MonoBehaviour
{
    [field: SerializeField] public NewUnit Unit { get; private set; }
    public NewTile Tile => Unit?.Tile;
    public BattleModel BattleModel { get; private set; }
    [SerializeField] private BattleModel defaultModel;
    
    [Header("Anchors")]
    [SerializeField] private Transform modelParent;
    [field:SerializeField] public Transform UiParent { get; private set; }
    
    public void SetUnit(NewUnit unit)
    {
        Unit = unit;

        var model = unit.Stats.So.model;
        if (model == null)
        {
            Debug.LogWarning($"{Unit.Stats.So} has no model, using default",unit.Stats.So);
            model = defaultModel;
        }
        
        BattleModel = Instantiate(model, modelParent);
    }
    
    
    #region DebugContextMenu
    #if UNITY_EDITOR
    
    [ContextMenu("DebugPassivesInstances")]
    public void DebugPassives()
    {
        Unit.DebugPassives();
    }
    
    [ContextMenu("Refresh Unit Stats Modifiers")]
    public void RefreshUnitStats()
    {
        Unit.Stats.RefreshModifiers();
    }
    
    #endif
    
    #endregion
    
}
