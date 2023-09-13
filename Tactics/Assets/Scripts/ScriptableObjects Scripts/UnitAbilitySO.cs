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
        [field: SerializeField] public bool IsInstantCast { get; private set; }

        public Func<Tile, bool> TileSelector { get; protected set; } = tile => tile != null;

        public void CastAbility(Unit caster, Tile[] targetTiles)
        {
            EventManager.Trigger(new StartAbilityCastEvent());

            Debug.Log($"{caster} is Casting! \n{targetTiles.Length} targets");
            
            AbilityEffect(caster, targetTiles);
        }

        protected abstract void AbilityEffect(Unit caster, Tile[] targetTiles);

        public UnitAbilityInstance CreateInstance()
        {
            return new UnitAbilityInstance(this);
        }
    }

    public class UnitAbilityInstance
    {
        public UnitAbilitySO SO { get; }
        public int ExpectedSelections => SO.ExpectedSelections;
        public int currentCooldown;
        public int CurrentSelectionCount => currentSelectedTiles.Count;

        private List<Tile> currentSelectedTiles = new ();

        public UnitAbilityInstance(UnitAbilitySO unitAbilitySo)
        {
            SO = unitAbilitySo;
            currentCooldown = 0;
            currentSelectedTiles.Clear();
        }

        public void CastAbility(Unit caster)
        {
            SO.CastAbility(caster,currentSelectedTiles.ToArray());
        }

        public void ClearTileSelection()
        {
            currentSelectedTiles.Clear();
        }

        public void AddTileToSelection(Tile tile)
        {
            if(currentSelectedTiles.Contains(tile)) return;
            currentSelectedTiles.Add(tile);
            
            // probably update ui or smt (callback ?)
            
            if(CurrentSelectionCount > SO.ExpectedSelections) RemoveTileFromSelection(currentSelectedTiles[0]);
        }

        public void RemoveTileFromSelection(Tile tile)
        {
            if(!currentSelectedTiles.Contains(tile)) return;
            currentSelectedTiles.Remove(tile);
            
            // probably update ui or smt (callback ?)
        }
    }
}