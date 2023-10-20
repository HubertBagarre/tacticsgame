using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects
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


        public IEnumerator AddPassive(Unit unit,UnitPassiveInstance instance)
        {
            return OnAddedEffect(unit,instance);
        }
        
        public IEnumerator RemovePassive(Unit unit,UnitPassiveInstance instance)
        {
            return OnRemovedEffect(unit,instance);
        }

        public IEnumerator EndTurnEffect(Unit unit,UnitPassiveInstance instance)
        {
            return OnTurnEndEffect(unit,instance);
        }
        
        public IEnumerator StartTurnEffect(Unit unit,UnitPassiveInstance instance)
        {
            return OnTurnStartEvent(unit,instance);
        }
        
        protected abstract IEnumerator OnAddedEffect(Unit unit,UnitPassiveInstance instance);
        protected abstract IEnumerator OnRemovedEffect(Unit unit,UnitPassiveInstance instance);
        protected abstract IEnumerator OnTurnEndEffect(Unit unit,UnitPassiveInstance instance);
        protected abstract IEnumerator OnTurnStartEvent(Unit unit,UnitPassiveInstance instance);

        public UnitPassiveInstance CreateInstance()
        {
            return new UnitPassiveInstance(this);
        }
    }
}

namespace Battle
{
    using ScriptableObjects;
    
    public class UnitPassiveInstance
    {
        public UnitPassiveSO SO { get; }
        public bool IsStackable => SO.IsStackable;
        public int CurrentStacks { get; private set; } = 0;
        public bool NeedRemoveOnTurnStart { get; private set; }
        public bool NeedRemoveOnTurnEnd { get; private set; }

        public event Action<int> OnCurrentStacksChanged;

        public UnitPassiveInstance(UnitPassiveSO so)
        {
            SO = so;
            NeedRemoveOnTurnEnd = false;
            NeedRemoveOnTurnStart = false;
        }
        
        public IEnumerator AddPassive(Unit unit)
        {
            if (SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks) return null;
            CurrentStacks++;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            return SO.AddPassive(unit,this);
        }
        
        public IEnumerator RemovePassive(Unit unit)
        {
            return SO.RemovePassive(unit,this);
        }

        public IEnumerator EndTurnEffect(Unit unit)
        {
            return SO.EndTurnEffect(unit,this);
        }
        
        public IEnumerator StartTurnEffect(Unit unit)
        {
            return SO.StartTurnEffect(unit,this);
        }

        public void SetRemoveOnTurnStart(bool value)
        {
            NeedRemoveOnTurnStart = value;
        }
        
        public void SetRemoveOnTurnEnd(bool value)
        {
            NeedRemoveOnTurnEnd = value;
        }
    }
}


