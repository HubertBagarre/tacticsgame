using UnityEngine;

public class Unit : MonoBehaviour
{
   [field:SerializeField] public Tile Tile { get; private set; }
   [field:SerializeField] public int Team { get; private set; } //0 is player
   [field:SerializeField] public UnitStatsSO Stats { get; private set; }

   [field:Header("Current Stats")]
   [field:SerializeField] public bool Active { get; private set; }
   [field:SerializeField] public int Movement { get; private set; }
   [field:SerializeField] public int Speed { get; private set; }
   public float DecayRate => Speed / 100f;

   public void InitUnit(Tile tile,int team,UnitStatsSO so)
   {
      Tile = tile;
      Team = team;
      Stats = so;

      Movement = so.BaseMovement;
      Speed = so.BaseSpeed;

      Active = true;

      tile.SetUnit(this);
   }

   public void SetTile(Tile tile)
   {
      Tile.RemoveUnit();
      
      Tile = tile;
      
      Tile.SetUnit(this);
   }
   
}
