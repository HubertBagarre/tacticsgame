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

        protected abstract IEnumerator OnAddedEffect(T container, PassiveInstance<T> instance);
        protected abstract IEnumerator OnRemovedEffect(T container, PassiveInstance<T> instance);

        public TPassiveInstance CreateInstance<TPassiveInstance>(int stacks = 1) where TPassiveInstance : PassiveInstance<T>, new()
        {
            var instance = new TPassiveInstance();
            instance.Init(this,stacks);
            return instance;
        }
    }

    public abstract class EntityPassiveSo<T> : PassiveSO<T> where T : IPassivesContainer<T>, IBattleEntity
    {
        [field: SerializeField] public bool RemoveOnTurnEnd { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnStart { get; private set; }= false;
        
        public IEnumerator EndTurnEffect(T container, EntityPassiveInstance<T> instance)
        {
            return OnTurnEndEffect(container, instance);
        }

        public IEnumerator StartTurnEffect(T container, EntityPassiveInstance<T> instance)
        {
            return OnTurnStartEvent(container, instance);
        }
        
        protected abstract IEnumerator OnTurnEndEffect(T container, EntityPassiveInstance<T> instance);
        protected abstract IEnumerator OnTurnStartEvent(T container, EntityPassiveInstance<T> instance);
    }
}

namespace Battle
{
    using ScriptableObjects;

    public interface IPassivesContainer<T> where T : IPassivesContainer<T>
    {
        public TPassiveInstance GetPassiveInstance<TPassiveInstance>(PassiveSO<T> passiveSo) where TPassiveInstance : PassiveInstance<T>;
        public IEnumerator AddPassiveEffect(PassiveSO<T> passiveSo, int amount = 1);
        public IEnumerator RemovePassiveEffect(PassiveSO<T> passiveSo);
        public IEnumerator RemovePassiveEffect(PassiveInstance<T> passiveInstance);
        public int GetPassiveEffectCount(Func<PassiveInstance<T>, bool> condition, out PassiveInstance<T> firstPassiveInstance);
    }
    
    public class PassiveInstance<T> where T : IPassivesContainer<T>
    {
        public PassiveSO<T> SO { get; private set; }
        public bool IsStackable => SO.IsStackable;
        public int CurrentStacks { get; private set; } = 0;
        public bool HasMoreStacksThanMax => SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks;

        private T associatedPassiveContainer;

        public event Action<int> OnCurrentStacksChanged;
        
        public virtual void Init(PassiveSO<T> so, int startingStacks = 1)
        {
            SO = so;
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

    public class EntityPassiveInstance<T> : PassiveInstance<T> where T : IPassivesContainer<T>,IBattleEntity
    {
        private EntityPassiveSo<T> castedSo;
        public bool NeedRemoveOnTurnStart { get; private set; }
        public bool NeedRemoveOnTurnEnd { get; private set; }

        public override void Init(PassiveSO<T> so, int startingStacks = 1)
        {
            base.Init(so, startingStacks);
            castedSo = (EntityPassiveSo<T>) so;
            NeedRemoveOnTurnEnd = castedSo.RemoveOnTurnEnd;
            NeedRemoveOnTurnStart = castedSo.RemoveOnTurnStart;
        }

        public void SetRemoveOnTurnStart(bool value)
        {
            NeedRemoveOnTurnStart = value;
        }
        
        public void SetRemoveOnTurnEnd(bool value)
        {
            NeedRemoveOnTurnEnd = value;
        }
        
        public IEnumerator EndTurnEffect(T container)
        {
            return castedSo.EndTurnEffect(container,this);
        }
        
        public IEnumerator StartTurnEffect(T container)
        {
            return castedSo.StartTurnEffect(container,this);
        }
    }
}