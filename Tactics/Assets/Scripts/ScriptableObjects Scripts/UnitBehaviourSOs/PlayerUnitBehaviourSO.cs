using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.UIEvents;
using Battle.UnitEvents;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitBehaviour/PlayerUnit")]
public class PlayerUnitBehaviourSO : UnitBehaviourSO
{
    private List<Tile> selectableTilesForMovement = new List<Tile>();
    
    public override void InitBehaviour(Unit unit)
    {
        EventManager.AddListener<EndUnitTurnEvent>(EndPlayerControl);
        
        EventManager.AddListener<UnitMovementStartEvent>(ClearSelectableTilesOnMovementStart);
        EventManager.AddListener<UnitMovementEndEvent>(UpdateAvailableUnitMovementTilesAfterMovementEnd);
        
        void EndPlayerControl(EndUnitTurnEvent ctx)
        {
            if (ctx.Unit != unit) return;
            
            EventManager.Trigger(new EndPlayerControlEvent());
        }
        
        void ClearSelectableTilesOnMovementStart(UnitMovementStartEvent _)
        {
            ResetTilesAppearance();
        }
        
        void UpdateAvailableUnitMovementTilesAfterMovementEnd(UnitMovementEndEvent ctx)
        {
            UpdateAvailableUnitMovementTiles(ctx.Unit);
        }
    }

    public override void RunBehaviour(Unit unit)
    {
        UpdateAvailableUnitMovementTiles(unit);
        
        EventManager.Trigger(new StartPlayerControlEvent());
    }
    
    private void UpdateAvailableUnitMovementTiles(Unit unit)
    {
        ResetTilesAppearance();
        
        selectableTilesForMovement.Clear();
            
        if (!unit.CanMove) return;
        if (unit.MovementLeft <= 0) return;

        SetSelectableTilesForMovement(unit.Tile, unit.MovementLeft, false, unit.Stats.WalkableTileSelector);

        InputManager.LeftClickEvent += MoveUnitOnClick;

        void MoveUnitOnClick()
        {
            MoveUnit(unit);

            InputManager.LeftClickEvent -= MoveUnitOnClick;
        }
    }
    
    private void ResetTilesAppearance()
    {
        foreach (var tile in tileM.AllTiles)
        {
            tile.SetAppearance(Tile.Appearance.Default);
            tile.SetPathRing(0);
        }
    }
    
    private void SetSelectableTilesForMovement(Tile origin, int range, bool includeDiag, Func<Tile, bool> extraCondition = null)
    {
        extraCondition ??= _ => true;

        selectableTilesForMovement.Add(origin);
        var justAdded = new List<Tile>() {origin};
        var iteration = 0;

        origin.SetPathRing(iteration);

        AddNeighbors();

        foreach (var tile in selectableTilesForMovement)
        {
            tile.SetAppearance(Tile.Appearance.Selectable);
        }

        void AddNeighbors()
        {
            if (iteration >= range) return;

            var neighbors = justAdded.SelectMany(tile => tile.GetDirectNeighbors(includeDiag)).Distinct().ToList();

            var validTiles = neighbors
                .Where(tile => !selectableTilesForMovement.Contains(tile))
                .Where(extraCondition)
                .ToList();

            selectableTilesForMovement.AddRange(validTiles);
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
        var destination = tileM.GetClickTile();

        if (!selectableTilesForMovement.Contains(destination)) return;

        var path = GetPathFromSelectableTiles(destination);

        ResetTilesAppearance();

        unit.MoveUnit(path);
    }
    
    private List<Tile> GetPathFromSelectableTiles(Tile destination)
    {
        if (!selectableTilesForMovement.Contains(destination)) return null;
            
        var path = new List<Tile>() {destination};
        var lastAdded = destination;

        for (int i = destination.PathRing - 1; i >= 1; i--)
        {
            lastAdded = lastAdded.GetDirectNeighbors().FirstOrDefault(tile => tile.PathRing == i);
            if (lastAdded == null) break;
            path.Add(lastAdded);
        }

        path.Reverse();

        return path;
    }
    
    
}

namespace Battle.UIEvents
{
    public class StartPlayerControlEvent {}
    public class EndPlayerControlEvent {}
}
