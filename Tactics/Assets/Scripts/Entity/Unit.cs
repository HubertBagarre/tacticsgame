using UnityEngine;

public class Unit : MonoBehaviour
{
   [field:SerializeField] public Tile Tile { get; private set; }

   public void SetTile(Tile tile)
   {
      Tile = tile;
      tile.SetUnit(this);
   }

}
