using UnityEngine;
using UnityEngine.Serialization;

public class Unit : MonoBehaviour
{
   [field:SerializeField] public Tile Tile { get; private set; }
   [field:SerializeField] public int Team { get; private set; } //0 is player
   [field:SerializeField] public UnitStatsSO Stats { get; private set; }

   [field:Header("Current Stats")]
   [field:SerializeField] public bool IsActive { get; private set; }
   [field:SerializeField] public int Movement { get; private set; }
   [field:SerializeField] public int Speed { get; private set; }
   public float DecayRate => Speed / 100f;
   public float TurnValue { get; private set; }

   public void InitUnit(Tile tile,int team,UnitStatsSO so)
   {
      Tile = tile;
      Team = team;
      Stats = so;

      Movement = so.BaseMovement;
      Speed = so.BaseSpeed;

      IsActive = true;

      tile.SetUnit(this);
   }

   public void SetTile(Tile tile)
   {
      Tile.RemoveUnit();
      
      Tile = tile;
      
      Tile.SetUnit(this);
   }

   public void ResetTurnValue(bool useInitiative = false)
   {
      TurnValue = useInitiative ? Stats.Initiative : 1000;
   }

   public void DecayTurnValue(float amount)
   {
      TurnValue -= amount * DecayRate;
   }

}
