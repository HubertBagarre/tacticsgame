using System.Collections.Generic;
using System.Linq;
using Battle.UIEvents;
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
            
            AbilityManager.OnUpdatedCastingAbility += UpdateAbilityTargetSelection;
            
            EventManager.AddListener<StartAbilityCastEvent>(ShowSelectedTilesOnStartAbilityCast);

            void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
            {
                ResetTileAppearance();
            }
            
            void ClearSelectableTilesOnEndAbilityTargetSelection(EndAbilityTargetSelectionEvent _)
            {
                Debug.Log("Clear tiles after tiles are selected");
                
                ResetTileAppearance();
            }
        }

        public void SetTiles(List<Tile> list)
        {
            tiles = list;
        }
        
        private void ResetTileAppearance()
        {
            foreach (var tile in AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
                tile.SetPathRing(0);
            }
        }
        
        private void UpdateAbilityTargetSelection(Unit caster,UnitAbilityInstance ability)
        {
            if (ability == null)
            {
                ResetTileAppearance();
                return;
            }
            
            foreach (var tile in AllTiles)
            {
                tile.SetAppearance(ability.IsTileSelectable(caster,tile) ? Tile.Appearance.Selectable : Tile.Appearance.Unselectable );
            }
        }
        
        private void ShowSelectedTilesOnStartAbilityCast(StartAbilityCastEvent ctx)
        {
            ResetTileAppearance();

            var selected = ctx.SelectedTiles;
            if(selected.Count <= 0) return;
            
            foreach (var tile in selected)
            {
                tile.SetAppearance(Tile.Appearance.Selected);
            }
        }
        
        private void ClickTile()
        {
            EventManager.Trigger(new ClickTileEvent(GetClickTile()));
        }
        
        private Tile GetClickTile()
        {
            InputManager.CastCamRay(out var tileHit, worldLayers);

            return tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
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