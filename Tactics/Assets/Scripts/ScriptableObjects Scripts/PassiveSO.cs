using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public enum PassiveType
    {
        Kit,Positive,Negative,Neutral
    }
    
    public abstract class PassiveSO<T> : ScriptableObject where T : IPassivesContainer<T>
    {
        [field: Header("Ability Details")]
        [field: SerializeField]
        public Sprite Sprite { get; private set; }

        [field: SerializeField] public GameObject Model { get; private set; }
        [field: SerializeField] public PassiveType Type { get; private set; } = PassiveType.Neutral;
        [field: SerializeField] public string Name { get; private set; }

        [field: SerializeField, TextArea(10, 10)]
        public string Description { get; private set; }

        [field: SerializeField] public bool IsStackable { get; private set; } = true;

        [field: SerializeField, Tooltip("0 is infinite")]
        public int MaxStacks { get; private set; } = 0; //0 is no limit

        [field: SerializeField] public bool HasStartTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool IsUnmovable { get; private set; }

        public IEnumerator AddPassive(T container, PassiveInstance<T> instance)
        {
            return OnAddedEffect(container, instance);
        }

        public IEnumerator RemovePassive(T container, PassiveInstance<T> instance)
        {
            return OnRemovedEffect(container, instance);
        }

        public IEnumerator EndTurnEffect(T container, PassiveInstance<T> instance)
        {
            return OnTurnEndEffect(container, instance);
        }

        public IEnumerator StartTurnEffect(T container, PassiveInstance<T> instance)
        {
            return OnTurnStartEvent(container, instance);
        }

        protected abstract IEnumerator OnAddedEffect(T container, PassiveInstance<T> instance);
        protected abstract IEnumerator OnRemovedEffect(T container, PassiveInstance<T> instance);
        protected abstract IEnumerator OnTurnEndEffect(T container, PassiveInstance<T> instance);
        protected abstract IEnumerator OnTurnStartEvent(T container, PassiveInstance<T> instance);

        public PassiveInstance<T> CreateInstance(int stacks = 1) => new PassiveInstance<T>(this, stacks);
    }
}

namespace Battle
{
    using ScriptableObjects;

    public interface IPassivesContainer<T> where T : IPassivesContainer<T>
    {
        public PassiveInstance<T> GetPassiveInstance(PassiveSO<T> passiveSo);
        public IEnumerator AddPassiveEffect(PassiveSO<T> passiveSo, int amount = 1);
        public IEnumerator RemovePassiveEffect(PassiveSO<T> passiveSo);
        public IEnumerator RemovePassiveEffect(PassiveInstance<T> passiveInstance);
        protected IEnumerator RemoveAllPassives();
        public int GetPassiveEffectCount(Func<PassiveInstance<T>, bool> condition, out PassiveInstance<T> firstPassiveInstance);
    }
    
    public class PassiveInstance<T> where T : IPassivesContainer<T>
    {
        public PassiveSO<T> SO { get; }
        public bool IsStackable => SO.IsStackable;
        public int CurrentStacks { get; private set; } = 0;
        public bool HasMoreStacksThanMax => SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks;
        public bool NeedRemoveOnTurnStart { get; private set; }
        public bool NeedRemoveOnTurnEnd { get; private set; }

        private T associatedPassiveContainer;

        public event Action<int> OnCurrentStacksChanged;

        public PassiveInstance(PassiveSO<T> so, int startingStacks = 1)
        {
            SO = so;
            NeedRemoveOnTurnEnd = false;
            NeedRemoveOnTurnStart = false;
            CurrentStacks = startingStacks - 1;
        }
        
        public IEnumerator AddPassive(T container,int amount = 1)
        {
            if(associatedPassiveContainer == null) associatedPassiveContainer = container;
            
            if (HasMoreStacksThanMax) return null;
            CurrentStacks += amount;
            if (HasMoreStacksThanMax) CurrentStacks = SO.MaxStacks;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            return SO.AddPassive(container,this);
        }
        
        public IEnumerator RemovePassive(T container)
        {
            return SO.RemovePassive(container,this);
        }

        public IEnumerator EndTurnEffect(T container)
        {
            return SO.EndTurnEffect(container,this);
        }
        
        public IEnumerator StartTurnEffect(T container)
        {
            return SO.StartTurnEffect(container,this);
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
            yield return AddPassive(associatedPassiveContainer,amount);
        }

        public IEnumerator DecreaseStacks(int amount)
        {
            if (CurrentStacks <= 1)
            {
                yield return associatedPassiveContainer.RemovePassiveEffect(this);
                yield break;
            }
            
            CurrentStacks -= amount;
            
            if(CurrentStacks <= 0) CurrentStacks = 0;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            
            yield return RemovePassive(associatedPassiveContainer);

        }
    }
}