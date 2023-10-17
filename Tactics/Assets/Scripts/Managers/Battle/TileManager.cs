using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Battle
{
    using UnitEvents;
    using InputEvent;
    using AbilityEvents;
    using ScriptableObjects.Ability;

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
            EventManager.AddListener<EndAbilityCastEvent>(ClearSelectedTilesOnCastEnd);

            void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
            {
                ResetTileAppearance();
            }
            
            void ClearSelectedTilesOnCastEnd(EndAbilityCastEvent _)
            {
                ResetTileAppearance();
            }
        }

        public void SetTiles(List<Tile> list)
        {
            tiles = list;
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
        
        private void UpdateAbilityTargetSelection(Unit caster,UnitAbilityInstance ability)
        {
            if (ability == null)
            {
                ResetTileAppearance();
                return;
            }
            
            foreach (var tile in AllTiles)
            {
                var selectable = ability.IsTileSelectable(caster,tile);
                tile.SetAppearance(selectable ? Tile.Appearance.Selectable : Tile.Appearance.Unselectable );
                //if(selectable) Debug.Log($"{tile} is selectable",tile.gameObject);
            }
            
            //TODO - find a way to show both selected and affected tiles
            foreach (var tile in ability.CurrentAffectedTiles)
            {
                tile.SetAppearance(Tile.Appearance.Affected);
            }
            
            foreach (var tile in ability.CurrentSelectedTiles)
            {
                tile.SetAppearance(Tile.Appearance.Selected);
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
            
            //Debug.Log($"Pew {tileHit.transform}",tileHit.transform);

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