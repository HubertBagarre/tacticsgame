using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    using AbilityEvent;

    public abstract class UnitAbilitySO : ScriptableObject
    {
        [field: Header("Ability Details")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int ExpectedSelections { get; private set; }
        [field: SerializeField] public int Cooldown { get; private set; }
        [field: SerializeField] public bool IsInstantCast { get; private set; } = false;
        [field: SerializeField] public bool EndUnitTurnAfterCast { get; private set; } = true;

        public Func<Tile, bool> TileSelector { get; protected set; } = tile => tile != null; // maybe tile => Func(tile) where Func(tile) is virtual

        public void CastAbility(Unit caster, Tile[] targetTiles)
        {
            AbilityEffect(caster, targetTiles);
        }

        protected abstract void AbilityEffect(Unit caster, Tile[] targetTiles);

        protected void EndAbility()
        {
            EventManager.Trigger(new EndAbilityCastEvent(this));
        }
        
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

        private List<Tile> currentSelectedTiles = new ();

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

        public void CastAbility(Unit caster)
        {
            EventManager.Trigger(new StartAbilityCastEvent(this,caster));
            
            ClearTileSelection();
            
            SO.CastAbility(caster,currentSelectedTiles.ToArray());
            
            currentSelectedTiles.Clear();
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
        }

        public void ClearTileSelection()
        {
            foreach (var tile in currentSelectedTiles)
            {
                tile.SetAppearance(Tile.Appearance.Default);
            }
        }

        public void AddTileToSelection(Tile tile)
        {
            if (currentSelectedTiles.Contains(tile))
            {
                RemoveTileFromSelection(tile);
                return;
            }
            currentSelectedTiles.Add(tile);
            
            tile.SetAppearance(Tile.Appearance.Selected);
            
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
            
            if(CurrentSelectionCount > SO.ExpectedSelections) RemoveTileFromSelection(currentSelectedTiles[0]);
        }

        public void RemoveTileFromSelection(Tile tile)
        {
            if(!currentSelectedTiles.Contains(tile)) return;
            currentSelectedTiles.Remove(tile);
            
            tile.SetAppearance(Tile.Appearance.Default);
            
            OnCurrentSelectedTilesUpdated?.Invoke(CurrentSelectionCount);
        }
    }
}