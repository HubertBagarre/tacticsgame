using Battle.BattleEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleEntityTimeline : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI turnValueText;
    [SerializeField] private GameObject turnValueObj;
    [SerializeField] private GameObject obj;
    
    [field:Header("Debug")]
    [field:SerializeField] public BattleEntity AssociatedEntity { get; private set; }
    
    public void ConnectToEntity(BattleEntity entity)
    {
        AssociatedEntity = entity;
        
        portraitImage.sprite = entity.Portrait;
        turnValueText.text = $"{AssociatedEntity.TurnValue}";

        EventManager.AddListener<UpdateTurnValuesEvent>(UpdateTurnValue);
        
        Show(false);
    }

    public void Disconnect()
    {
        EventManager.RemoveListener<UpdateTurnValuesEvent>(UpdateTurnValue);
    }

    public void Show(bool value)
    {
        obj.SetActive(value);
    }

    private void UpdateTurnValue(UpdateTurnValuesEvent ctx)
    {
        turnValueText.text = $"{(int)AssociatedEntity.TurnValue}";
        
        turnValueObj.SetActive(AssociatedEntity.TurnValue > 0);
    }

}
