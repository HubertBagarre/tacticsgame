using UnityEngine;

public class Unit : MonoBehaviour
{
   [field:SerializeField] public Tile Tile { get; private set; }
   [field:SerializeField] public int Team { get; private set; } //0 is player
   [field:SerializeField] public UnitStatsSO Stats { get; private set; }

   [field:Header("Current Stats")]
   [field:SerializeField] public int Movement { get; private set; }

   public void InitUnit(Tile tile,int team,UnitStatsSO so)
   {
      Tile = tile;
      Team = team;
      Stats = so;

      Movement = so.movement;

      tile.SetUnit(this);
   }

   public void SetTile(Tile tile)
   {
      Tile = tile;
   }

}
