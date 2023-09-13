using System;
using Battle;

using UnityEngine;

namespace Battle
{
    using AbilityEvent;
    
    public abstract class UnitAbilitySO : ScriptableObject
    {
        [field:SerializeField] public Sprite Sprite { get; private set; }
        [field:SerializeField] public string Name { get; private set; }
        [field:SerializeField] public string Description { get; private set; }
        [field:SerializeField] public int ExpectedSelections { get; private set; }
        [field:SerializeField] public int Cooldown { get; private set; }
        
        public Func<Tile, bool> TileSelector { get; protected set; } = tile => tile != null;
        public Func<Unit, bool> UnitSelector { get; protected set; } = unit => unit != null;
        
        public void CastAbility(Unit[] targetUnits,Tile[] targetTiles)
        {
            EventManager.Trigger(new StartAbilityCast());
            
            AbilityEffect(targetUnits,targetTiles);
        }
    
        protected abstract void AbilityEffect(Unit[] targetUnits,Tile[] targetTiles);
    }

    public class UnitAbilityInstance
    {
        public UnitAbilitySO SO { get; }
        public int currentCooldown;

        public UnitAbilityInstance(UnitAbilitySO unitAbilitySo)
        {
            SO = unitAbilitySo;
        }
    }
}


