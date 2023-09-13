using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using UnitEvents;

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
            EventManager.AddListener<EndUnitTurnEvent>(ClearSelectableTilesOnTurnEnd);
            
            void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
            {
                ResetTileAppearance();
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