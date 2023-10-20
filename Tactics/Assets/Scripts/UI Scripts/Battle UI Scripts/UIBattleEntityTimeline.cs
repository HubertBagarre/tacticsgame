using Battle.BattleEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleEntityTimeline : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI turnValueText;
    [SerializeField] private GameObject turnValueObj;
    [SerializeField] private GameObject obj;
    
    [field:Header("Debug")]
    [field:SerializeField] public IBattleEntity AssociatedEntity { get; private set; }
    
    public void ConnectToEntity(IBattleEntity entity)
    {
        AssociatedEntity = entity;
        borderImage.color = AssociatedEntity.Team == 0 ? Color.cyan : Color.red;
        
        gameObject.name = $"{AssociatedEntity}'s time";
        
        portraitImage.sprite = AssociatedEntity.Portrait;
        turnValueText.text = $"{AssociatedEntity.DistanceFromTurnStart}";

        EventManager.AddListener<UpdateTurnValuesEvent>(UpdateTurnValue);
        
        Show(false);

        entity.OnDeath += DestroySelf;
    }

    private void DestroySelf()
    {
        Disconnect();
        gameObject.SetActive(false);
    }

    public void Disconnect()
    {
        EventManager.RemoveListener<UpdateTurnValuesEvent>(UpdateTurnValue);
    }

    public void Show(bool value)
    {
        obj.SetActive(value);
    }

    public void SetPreview(bool value)
    {
        var col = portraitImage.color;
        col.a = value ? 0.7f : 1f;
        portraitImage.color = col;
    }
    
    private void UpdateTurnValue(UpdateTurnValuesEvent ctx)
    {
        portraitImage.sprite = AssociatedEntity.Portrait;
        
        turnValueText.text = $"{(int)AssociatedEntity.TurnOrder}";
        
        turnValueObj.SetActive(AssociatedEntity.DistanceFromTurnStart > 0);
    }

}
