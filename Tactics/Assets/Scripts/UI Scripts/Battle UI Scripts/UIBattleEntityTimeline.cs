using Battle;
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
    
    public TimelineEntity TimelineEntity { get; private set; }
     public IBattleEntity AssociatedEntity { get; private set; }
    
    public void ConnectToEntity(IBattleEntity entity)
    {
        AssociatedEntity = entity;
        ChangeBorderColor(AssociatedEntity.Team); 
        
        gameObject.name = $"{AssociatedEntity}'s time";
        
        ChangeImage(AssociatedEntity.Portrait);
        ChangeValue((int)AssociatedEntity.DistanceFromTurnStart);

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
    
    public void ChangeValue(int value)
    {
        turnValueText.text = $"{value}";
        turnValueObj.SetActive(value >= 0);
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
        ChangeImage(AssociatedEntity.Portrait);
        ChangeValue((int)AssociatedEntity.TurnOrder);
    }

}
