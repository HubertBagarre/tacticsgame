using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    public enum PassiveType
    {
        Kit,Buff,Debuff
    }
    
    public abstract class UnitPassiveSO : ScriptableObject
    {
        [field: Header("Ability Details")]
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public PassiveType Type { get; private set; }
        [field: SerializeField,TextArea(10,10)] public string Description { get; private set; }
        [field: SerializeField] public bool IsStackable { get; private set; } = true;
        [field: SerializeField,Tooltip("0 is infinite")] public int MaxStacks { get; private set; } = 0; //0 is no limit
        [field: SerializeField] public bool HasStartTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool IsUnmovable { get; private set; }


        public IEnumerator AddPassive(Unit unit)
        {
            return OnAddedEffect(unit);
        }
        
        public IEnumerator RemovePassive(Unit unit)
        {
            return OnRemovedEffect(unit);
        }

        public IEnumerator EndTurnEffect(Unit unit)
        {
            return OnTurnEndEffect(unit);
        }
        
        public IEnumerator StartTurnEffect(Unit unit)
        {
            return OnTurnStartEvent(unit);
        }
        
        protected abstract IEnumerator OnAddedEffect(Unit unit);
        protected abstract IEnumerator OnRemovedEffect(Unit unit);
        protected abstract IEnumerator OnTurnEndEffect(Unit unit);
        protected abstract IEnumerator OnTurnStartEvent(Unit unit);

        public UnitPassiveInstance CreateInstance()
        {
            return new UnitPassiveInstance(this);
        }
    }
    

    public class UnitPassiveInstance
    {
        public UnitPassiveSO SO { get; }
        public bool IsStackable => SO.IsStackable;
        public int CurrentStacks { get; private set; } = 0;

        public event Action<int> OnCurrentStacksChanged;

        public UnitPassiveInstance(UnitPassiveSO so)
        {
            SO = so;
        }
        
        public IEnumerator AddPassive(Unit unit)
        {
            if (SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks) return null;
            CurrentStacks++;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            return SO.AddPassive(unit);
        }
        
        public IEnumerator RemovePassive(Unit unit)
        {
            return SO.RemovePassive(unit);
        }

        public IEnumerator EndTurnEffect(Unit unit)
        {
            return SO.EndTurnEffect(unit);
        }
        
        public IEnumerator StartTurnEffect(Unit unit)
        {
            return SO.StartTurnEffect(unit);
        }
    }
}


