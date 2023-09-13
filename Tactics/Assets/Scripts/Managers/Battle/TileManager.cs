using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using InputEvent;
    using AbilityEvent;

    /// <summary>
    /// Handles Tiles
    ///
    /// List all tiles
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] protected LayerMask worldLayers;

        [Header("Debug")] [SerializeField] private List<Tile> tiles = new List<Tile>();

        public List<Tile> AllTiles => tiles.ToList();
        
        private void Start()
        {
            InputManager.LeftClickEvent += ClickTile;
            
            EventManager.AddListener<EndUnitTurnEvent>(ClearSelectableTilesOnTurnEnd);

            void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
            {
                ResetTileAppearance();
            }
            

            void ClickTile()
            {
                EventManager.Trigger(new ClickTileEvent(GetClickTile()));
            }
        }

        public void SetTiles(List<Tile> list)
        {
            tiles = list;
        }

        public Tile GetClickTile()
        {
            InputManager.CastCamRay(out var tileHit, worldLayers);

            return tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
        }

        private void ResetTileAppearance()
        {
            foreach (var tile in tiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
                tile.SetPathRing(0);
            }
        }
    }
}

namespace Battle.InputEvent
{
    public class ClickTileEvent
    {
        public Tile Tile { get; }

        public ClickTileEvent(Tile tile)
        {
            Tile = tile;
        }
    }
}