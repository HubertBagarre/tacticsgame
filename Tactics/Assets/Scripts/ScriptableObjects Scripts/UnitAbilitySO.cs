using System;
using System.Collections;
using System.Collections.Generic;
using Battle.UnitEvents;
using UnityEngine;

namespace Battle
{
    using AbilityEvents;

    public abstract class UnitAbilitySO : ScriptableObject
    {
        [field: Header("Ability Details")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int ExpectedSelections { get; private set; }
        [field: SerializeField] public int Cooldown { get; private set; }
        [field: SerializeField] public bool SkipTargetSelection { get; private set; } = false;
        [field: SerializeField] public bool SkipTargetConfirmation { get; private set; } = false;
        [field: SerializeField] public bool EndUnitTurnAfterCast { get; private set; } = true;

        public bool IsTileSelectable(Unit caster, Tile tile, List<Tile> currentlySelectedTiles)
        {
            return TileSelectionMethod(caster, tile, currentlySelectedTiles);
        }

        protected virtual bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return selectableTile != null;
        }

        public IEnumerator CastAbility(Unit caster, Tile[] targetTiles)
        {
            return AbilityEffect(caster, targetTiles);
        }

        protected abstract IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles);

        public UnitAbilityInstance CreateInstance()
        {
            return new UnitAbilityInstance(this);
        }
    }

    public class UnitAbilityInstance
    {
        public UnitAbilitySO SO { get; }
        public int ExpectedSelections => SO.ExpectedSelections;
        public int SelectionsLeft => SO.ExpectedSelections - CurrentSelectionCount;
        public int CurrentCooldown { get; private set; }
        public int CurrentSelectionCount => currentSelectedTiles.Count;

        public bool IsTileSelectable(Unit caster, Tile tile) => SO.IsTileSelectable(caster, tile, currentSelectedTiles);

        private List<Tile> currentSelectedTiles = new();

        public event Action<int> OnCurrentSelectedTilesUpdated;
        public event Action<int> OnCurrentCooldownUpdated;

        public UnitAbilityInstance(UnitAbilitySO unitAbilitySo)
        {
            SO = unitAbilitySo;
            CurrentCooldown = 0;
            currentSelectedTiles.Clear();
        }

        public override string ToString()
        {
            return $"{SO.name} (Instance)";
        }

        public void ClearSelection()
        {
            currentSelectedTiles.Clear();
        }

        public void CastAbility(Unit caster)
        {
            EventManager.Trigger(new StartAbilityCastEvent(this, caster, currentSelectedTiles));

            caster.StartCoroutine(AbilityCast());

            IEnumerator AbilityCast()
            {
                yield return caster.StartCoroutine(SO.CastAbility(caster, currentSelectedTiles.ToArray()));

                currentSelectedTiles.Clear();
                OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
                
                EventManager.Trigger(new EndAbilityCastEvent(SO));
            }
        }

        public void AddTileToSelection(Unit caster, Tile tile)
        {
            if (!IsTileSelectable(caster, tile)) return;

            if (currentSelectedTiles.Contains(tile))
            {
                RemoveTileFromSelection(caster, tile);
                return;
            }

            currentSelectedTiles.Add(tile);

            tile.SetAppearance(Tile.Appearance.Selected);

            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);

            if (CurrentSelectionCount > SO.ExpectedSelections) RemoveTileFromSelection(caster, currentSelectedTiles[0]);

            if (SO.SkipTargetConfirmation)
            {
                EventManager.Trigger(new EndAbilityTargetSelectionEvent(false));
            }
        }

        public void RemoveTileFromSelection(Unit caster, Tile tile)
        {
            if (!currentSelectedTiles.Contains(tile)) return;
            currentSelectedTiles.Remove(tile);

            tile.SetAppearance(IsTileSelectable(caster, tile)
                ? Tile.Appearance.Selectable
                : Tile.Appearance.Unselectable);

            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
        }
    }
}