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
    [field:SerializeField,SerializeReference] public IBattleEntity AssociatedEntity { get; private set; }
    
    public void ConnectToEntity(IBattleEntity entity)
    {
        AssociatedEntity = entity;
        ChangeBorderColor(AssociatedEntity.Team); 
        
        gameObject.name = $"{AssociatedEntity}'s time";
        
        ChangeImage(AssociatedEntity.Portrait);
        ChangeValue(AssociatedEntity.DistanceFromTurnStart);

        EventManager.AddListener<UpdateTurnValuesEvent>(UpdateTurnValue);
        
        Show(false);

        entity.OnDeath += DestroySelf;
    }

    private void DestroySelf()
    {
        Disconnect();
        gameObject.SetActive(false);
    }

    public void ChangeImage(Sprite sprite)
    {
        portraitImage.sprite = sprite;
    }

    public void ChangeBorderColor(int team)
    {
        borderImage.color = team == 0 ? Color.cyan : Color.red;
    }
    
    public void ChangeValue(float value)
    {
        turnValueText.text = $"{value}";
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
