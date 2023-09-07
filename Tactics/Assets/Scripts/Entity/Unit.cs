using UnityEngine;

public class Unit : MonoBehaviour
{
   [field:SerializeField] public Tile Tile { get; private set; }
   [field:SerializeField] public int Team { get; private set; } //0 is player

   public void InitUnit(Tile tile,int team)
   {
      Tile = tile;
      Team = team;
      tile.SetUnit(this);
   }

}
