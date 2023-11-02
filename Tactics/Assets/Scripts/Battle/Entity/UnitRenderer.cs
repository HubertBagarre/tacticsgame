using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

public class UnitRenderer : MonoBehaviour
{
    public NewUnit Unit { get; private set; }
    public Tile Tile => Unit.Tile;
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
    
    
    
}
