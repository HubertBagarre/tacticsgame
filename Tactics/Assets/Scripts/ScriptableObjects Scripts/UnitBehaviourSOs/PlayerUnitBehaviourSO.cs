using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using UIEvents;
    using UnitEvents;
    
    [CreateAssetMenu(menuName = "Battle Scriptables/UnitBehaviour/PlayerUnit")]
    public class PlayerUnitBehaviourSO : UnitBehaviourSO
    {
        private List<Tile> selectableTilesForMovement = new List<Tile>();
        private Unit controlledUnit;

        public override void InitBehaviour(Unit unit)
        {
            controlledUnit = unit;

            EventManager.AddListener<EndUnitTurnEvent>(EndPlayerControl);

            EventManager.AddListener<UnitMovementStartEvent>(ClearSelectableTilesOnMovementStart);
            
            void EndPlayerControl(EndUnitTurnEvent ctx)
            {
                if (ctx.Unit != unit) return;

                EventManager.RemoveListener<UnitMovementEndEvent>(UpdateAvailableUnitMovementTilesAfterMovementEnd);

                EventManager.Trigger(new EndPlayerControlEvent());
            }

            void ClearSelectableTilesOnMovementStart(UnitMovementStartEvent _)
            {
                ResetTilesAppearance();
            }
        }

        public override void ShowBehaviourPreview(Unit unit)
        {
        }

        public override void RunBehaviour(Unit unit)
        {
            //UpdateAvailableUnitMovementTiles(unit);

            //EventManager.AddListener<EndPlayerControlEvent>(RemoveMouseInputs, true);

            EventManager.Trigger(new StartPlayerControlEvent(unit));
            
            //EventManager.Trigger(new StartUnitMovementSelectionEvent(unit));
        }

        private void RemoveMouseInputs(EndPlayerControlEvent _)
        {
            InputManager.LeftClickEvent -= MoveUnitOnClick;
        }

        #region Unit Movement

        private void MoveUnitOnClick()
        {
            if (!TryMoveUnit(controlledUnit)) return;

            EventManager.AddListener<UnitMovementEndEvent>(UpdateAvailableUnitMovementTilesAfterMovementEnd, true);

            RemoveMouseInputs(null);
        }

        private void UpdateAvailableUnitMovementTiles(Unit unit)
        {
            ResetTilesAppearance();

            selectableTilesForMovement.Clear();

            if (!unit.CanMove) return;
            if (unit.MovementLeft <= 0) return;

            SetSelectableTilesForMovement(unit.Tile, unit.MovementLeft, false, unit.Stats.WalkableTileSelector);

            InputManager.LeftClickEvent += MoveUnitOnClick;
        }

        private void UpdateAvailableUnitMovementTilesAfterMovementEnd(UnitMovementEndEvent ctx)
        {
            if (ctx.Unit != controlledUnit) return;

            UpdateAvailableUnitMovementTiles(ctx.Unit);
        }

        private void ResetTilesAppearance()
        {
            foreach (var tile in tileM.AllTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
                tile.SetPathRing(0);
            }
        }

        private void SetSelectableTilesForMovement(Tile origin, int range, bool includeDiag,
            Func<Tile, bool> extraCondition = null)
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

        private bool TryMoveUnit(Unit unit)
        {
            var destination = tileM.GetClickTile();

            if (!selectableTilesForMovement.Contains(destination)) return false;

            var path = GetPathFromSelectableTiles(destination);

            ResetTilesAppearance();

            unit.MoveUnit(path);

            return true;
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

        #endregion

        #region Unit Abilities

        #endregion
    }
}


namespace Battle.UIEvents
{
    public class StartPlayerControlEvent
    {
        public Unit PlayerUnit { get; }

        public StartPlayerControlEvent(Unit playerUnit)
        {
            PlayerUnit = playerUnit;
        }
    }

    public class EndPlayerControlEvent
    {
    }

    public class StartUnitMovementSelectionEvent
    {
        public Unit Unit { get; }

        public StartUnitMovementSelectionEvent(Unit unit)
        {
            Unit = unit;
        }
    }

    public class EndUnitMovementSelectionEvent
    {
        
    }
}