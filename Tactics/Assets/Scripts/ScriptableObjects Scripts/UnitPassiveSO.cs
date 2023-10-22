using System;
using System.Collections;
using UnityEngine;

/*
namespace Battle.ScriptableObjects
{
    
    
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

        public UnitPassiveInstance CreateInstance(int stacks = 1)
        {
            return new UnitPassiveInstance(this,stacks);
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
        public bool HasMoreStacksThanMax => SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks;
        public bool NeedRemoveOnTurnStart { get; private set; }
        public bool NeedRemoveOnTurnEnd { get; private set; }

        private Unit associatedUnit;
        
        public event Action<int> OnCurrentStacksChanged;

        public UnitPassiveInstance(UnitPassiveSO so,int startingStacks = 1)
        {
            SO = so;
            NeedRemoveOnTurnEnd = false;
            NeedRemoveOnTurnStart = false;
            CurrentStacks = startingStacks - 1;
        }
        
        public IEnumerator AddPassive(Unit unit,int amount = 1)
        {
            if(associatedUnit == null) associatedUnit = unit;
            
            if (HasMoreStacksThanMax) return null;
            CurrentStacks += amount;
            if (HasMoreStacksThanMax) CurrentStacks = SO.MaxStacks;
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

        public IEnumerator IncreaseStacks(int amount)
        {
            yield return AddPassive(associatedUnit,amount);
        }

        public IEnumerator DecreaseStacks(int amount)
        {
            if (CurrentStacks <= 1)
            {
                yield return associatedUnit.RemovePassiveEffect(this);
                yield break;
            }
            
            CurrentStacks -= amount;
            
            if(CurrentStacks <= 0) CurrentStacks = 0;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            
            yield return RemovePassive(associatedUnit);

        }
    }
}
*/

