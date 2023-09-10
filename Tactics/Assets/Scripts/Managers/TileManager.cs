using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public List<Tile> SelectableTiles { get; private set; } = new List<Tile>();

    private void Start()
    {
        EventManager.AddListener<StartUnitTurnEvent>(UpdateAvailableUnitMovementTilesAfterTurnStart);
        EventManager.AddListener<EndUnitTurnEvent>(ClearSelectableTilesOnTurnEnd);
        
        EventManager.AddListener<UnitMovementEndEvent>(UpdateAvailableUnitMovementTilesAfterMovementEnd);
        EventManager.AddListener<UnitMovementStartEvent>(ClearSelectableTilesOnMovementStart);

        void ClearSelectableTilesOnTurnEnd(EndUnitTurnEvent _)
        {
            ClearSelectableTiles();
        }
        
        void ClearSelectableTilesOnMovementStart(UnitMovementStartEvent _)
        {
            ClearSelectableTiles();
        }
    }

    public void SetTiles(List<Tile> list)
    {
        tiles = list;
    }

    private Tile GetClickTile()
    {
        InputManager.CastCamRay(out var tileHit, worldLayers);

        return tileHit.transform != null ? tileHit.transform.GetComponent<Tile>() : null;
    }
    
    private void ClearSelectableTiles()
    {
        SelectableTiles.Clear();
        
        foreach (var tile in tiles)
        {
            tile.SetAppearance(Tile.Appearance.Default);
            tile.SetPathRing(0);
        }
    }

    private void UpdateAvailableUnitMovementTilesAfterTurnStart(StartUnitTurnEvent ctx)
    {
        UpdateAvailableUnitMovementTiles(ctx.Unit);
    }
    
    private void UpdateAvailableUnitMovementTilesAfterMovementEnd(UnitMovementEndEvent ctx)
    {
        UpdateAvailableUnitMovementTiles(ctx.Unit);
    }
    
    private void UpdateAvailableUnitMovementTiles(Unit unit)
    {
        ClearSelectableTiles();
        
        if(!unit.IsPlayerControlled) return;
        if (!unit.CanMove) return;
        if(unit.MovementLeft <= 0) return;

        SetSelectableTilesForMovement(unit.Tile, unit.MovementLeft, false, unit.Stats.WalkableTileSelector);

        InputManager.LeftClickEvent += MoveUnitOnClick;
        
        void MoveUnitOnClick()
        {
            MoveUnit(unit);
            
            InputManager.LeftClickEvent -= MoveUnitOnClick;
        }
    }

    private void SetSelectableTilesForMovement(Tile origin,int range,bool includeDiag,Func<Tile,bool> extraCondition = null)
    {
        extraCondition ??= _ => true;

        SelectableTiles.Add(origin);
        var justAdded = new List<Tile>() {origin};
        var iteration = 0;
        
        origin.SetPathRing(iteration);
        
        AddNeighbors();
        
        foreach (var tile in SelectableTiles)
        {
            tile.SetAppearance(Tile.Appearance.Selectable);
        }
        
        void AddNeighbors()
        {
            if(iteration >= range) return;
            
            var neighbors = justAdded.SelectMany(tile => tile.GetDirectNeighbors(includeDiag)).Distinct().ToList();

            var validTiles = neighbors
                .Where(tile => !SelectableTiles.Contains(tile))
                .Where(extraCondition)
                .ToList();

            SelectableTiles.AddRange(validTiles);
            justAdded = validTiles;
            
            iteration++;
            
            foreach (var addedTile in justAdded)
            {
                addedTile.SetPathRing(iteration);
            }
            
            AddNeighbors();
        }
    }
    
    private void MoveUnit(Unit unit)
    {
        var destination = GetClickTile();
        
        if(!SelectableTiles.Contains(destination)) return;
        
        var path = GetPathFromSelectableTiles(destination);
        
        ClearSelectableTiles();

        unit.MoveUnit(path);
    }

    public List<Tile> GetPathFromSelectableTiles(Tile destination)
    {
        if (!SelectableTiles.Contains(destination)) return null;
        
        Debug.Log($"Getting path to {destination}");

        var path = new List<Tile>() {destination};
        var lastAdded = destination;
        
        for (int i = destination.PathRing - 1; i >= 1; i--)
        {
            Debug.Log($"last added : {lastAdded}",lastAdded);
            lastAdded = lastAdded.GetDirectNeighbors().FirstOrDefault(tile => tile.PathRing == i);
            if(lastAdded == null) break;
            path.Add(lastAdded);
        }

        path.Reverse();
        
        return path;
    }
}