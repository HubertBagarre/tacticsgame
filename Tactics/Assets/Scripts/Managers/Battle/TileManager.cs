using System.Collections.Generic;
using System.Linq;
using Battle.InputEvent;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Handles Tiles
    ///
    /// List all tiles
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] protected LayerMask worldLayers;

        [Header("Debug")] [SerializeField] private List<Tile> tiles = new List<Tile>();
        private readonly List<NewTile> newTiles = new List<NewTile>();
        private readonly Dictionary<NewTile,Tile> tileRenderers = new (); //TODO - replace with TileRenderer (instead of Tile)

        public IReadOnlyList<NewTile> NewTiles => newTiles;
        public List<Tile> AllTiles => tiles.ToList();
        
        public void AddCallbacks()
        {
            InputManager.LeftClickEvent += ClickTile;
            
            EventManager.AddListener<StartAbilityTargetSelectionEvent>(ResetTileAppearance);
            EventManager.AddListener<StartAbilityTargetSelectionEvent>(AddAbilityCallbacks);
            EventManager.AddListener<EndAbilityTargetSelectionEvent>(RemoveAbilityCallbacks);
            
            /*
            
            EventManager.AddListener<EndUnitTurnEvent>(ClearSelectableTilesOnTurnEnd);
            
            AbilityManager.OnUpdatedCastingAbility += UpdateAbilityTargetSelection;
            
            EventManager.AddListener<StartAbilityCastEvent>(ShowSelectedTilesOnStartAbilityCast);
            EventManager.AddListener<EndAbilityCastEvent>(ClearSelectedTilesOnCastEnd);

            return;
            
            void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
            {
                ResetTileAppearance();
            }
            
            void ClearSelectedTilesOnCastEnd(EndAbilityCastEvent _)
            {
                ResetTileAppearance();
            }*/
        }

        public void RemoveCallbacks()
        {
            InputManager.LeftClickEvent -= ClickTile;
            
            EventManager.RemoveListener<StartAbilityTargetSelectionEvent>(ResetTileAppearance);
            EventManager.RemoveListener<StartAbilityTargetSelectionEvent>(AddAbilityCallbacks);
            EventManager.RemoveListener<EndAbilityTargetSelectionEvent>(RemoveAbilityCallbacks);
            
            /*
            AbilityManager.OnUpdatedCastingAbility -= UpdateAbilityTargetSelection;
            
            EventManager.RemoveListener<StartAbilityCastEvent>(ShowSelectedTilesOnStartAbilityCast);*/
        }

        public void SetTiles(List<Tile> list)
        {
            tiles = list;

            //TODO - This should create NewTiles and instantiate tiles (now creates new tile based on old tile)
            foreach (var tile in tiles)
            {
                var t = new NewTile(tile.Position,tile);
                newTiles.Add(t);
                tileRenderers.Add(t,tile);
            }
            
            ResetTileAppearance();
        }

        private void ResetTileAppearance(StartAbilityTargetSelectionEvent _)
        {
            ResetTileAppearance();
        }
        
        private void ResetTileAppearance()
        {
            foreach (var tile in AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
                tile.HideBorders();
                tile.HideLineRendererPath();
            }
        }
        
        private void UpdateSelectedTiles(AbilityInstance ability)
        {
            if (ability == null)
            {
                ResetTileAppearance();
                return;
            }
            
            foreach (var tile in NewTiles)
            {
                tileRenderers[tile].SetAppearance(ability.IsTileSelectable(tile)
                    ? Tile.Appearance.Selectable
                    : Tile.Appearance.Unselectable);
            }
            
            //TODO - find a way to show both selected and affected tiles
            foreach (var tile in ability.CurrentAffectedTiles)
            {
                tileRenderers[tile].SetAppearance(Tile.Appearance.Affected);
            }
            
            foreach (var tile in ability.CurrentSelectedTiles)
            {
                tileRenderers[tile].SetAppearance(Tile.Appearance.Selected);
            }
        }
        
        private void AddAbilityCallbacks(StartAbilityTargetSelectionEvent ctx)
        {
            var ability = ctx.AbilityInstance;
            
            ability.OnCurrentSelectedTilesUpdated += UpdateSelectedTiles;
        }
        
        private void RemoveAbilityCallbacks(EndAbilityTargetSelectionEvent ctx)
        {
            ResetTileAppearance();
            
            var ability = ctx.AbilityInstance;
            
            ability.OnCurrentSelectedTilesUpdated -= UpdateSelectedTiles;
        }
        
        private void ClickTile()
        {
            EventManager.Trigger(new ClickTileEvent(GetClickTile()));
        }
        
        private NewTile GetClickTile()
        {
            InputManager.CastCamRay(out var tileHit, worldLayers);
            
            //Debug.Log($"Pew {tileHit.transform}",tileHit.transform);

            if (tileHit.transform == null) return null;
            
            var tileRenderer = tileHit.transform.GetComponent<Tile>(); //TODO - replace with TileRenderer (instead of Tile)
            
            var tile = tileRenderer != null ? tileRenderer.NewTile : null;
            
            //Debug.Log($"Hit tile {tile}");
            
            return tile;
        }
    }
}

namespace Battle.InputEvent
{
    public class ClickTileEvent
    {
        public NewTile Tile { get; }

        public ClickTileEvent(NewTile tile)
        {
            Tile = tile;
        }
    }
}