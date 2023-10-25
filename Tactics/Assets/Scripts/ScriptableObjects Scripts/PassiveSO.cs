using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public enum PassiveType
    {
        Kit,Positive,Negative,Neutral
    }
    
    public abstract class PassiveSO<T> : ScriptableObject where T : MonoBehaviour, IPassivesContainer<T>
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
        
        [field: SerializeField] public bool IsUnmovable { get; private set; }

        public virtual IEnumerator AddPassive(T container, PassiveInstance<T> instance)
        {
            return OnAddedEffect(container, instance);
        }

        public virtual IEnumerator RemovePassive(T container, PassiveInstance<T> instance)
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

    public abstract class RoundPassiveSo<T> : PassiveSO<T> where T : MonoBehaviour, IPassivesContainer<T>
    {
        [field: SerializeField] public bool HasStartRoundEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndRoundEffect { get; private set; } = false;
        
        public override IEnumerator AddPassive(T container, PassiveInstance<T> instance)
        {
            if(HasStartRoundEffect) BattleManager.AddStartRoundPassive(RoundStartEffect(container, instance));
            if(HasEndRoundEffect) BattleManager.AddEndRoundPassive(RoundEndEffect(container, instance));
            
            return base.AddPassive(container, instance);
        }

        public override IEnumerator RemovePassive(T container, PassiveInstance<T> instance)
        {
            if(HasStartRoundEffect) BattleManager.RemoveStartRoundPassive(RoundStartEffect(container, instance));
            if(HasEndRoundEffect) BattleManager.RemoveEndRoundPassive(RoundEndEffect(container, instance));
            
            return base.RemovePassive(container, instance);
        }

        protected abstract IEnumerator RoundStartEffect(T container, PassiveInstance<T> instance);
        protected abstract IEnumerator RoundEndEffect(T container, PassiveInstance<T> instance);
    }

    public abstract class TilePassiveSo<T> : PassiveSO<T> where T : Tile, IPassivesContainer<T>
    {
        [field: SerializeField] public bool HasUnitEnterEffect { get; private set; } = false;
        [field: SerializeField] public bool HasUnitExitEffect { get; private set; } = false;
        
        public override IEnumerator AddPassive(T tile, PassiveInstance<T> instance)
        {
            if(HasUnitEnterEffect) tile.AddOnUnitEnterEvent(UnitEnterEffect(tile, instance));
            if(HasUnitExitEffect) tile.AddOnUnitExitEvent(UnitExitEffect(tile, instance));
            
            return base.AddPassive(tile, instance);
        }

        public override IEnumerator RemovePassive(T tile, PassiveInstance<T> instance)
        {
            if(HasUnitEnterEffect) tile.RemoveOnUnitEnterEvent(UnitEnterEffect(tile, instance));
            if(HasUnitExitEffect) tile.RemoveOnUnitExitEvent(UnitExitEffect(tile, instance));
            
            return base.RemovePassive(tile, instance);
        }
        
        protected abstract IEnumerator UnitEnterEffect(Tile tile, PassiveInstance<T> instance);
        protected abstract IEnumerator UnitExitEffect(Tile tile, PassiveInstance<T> instance);
    }
    
    public abstract class EntityPassiveSo<T> : PassiveSO<T> where T : MonoBehaviour, IPassivesContainer<T>, IBattleEntity
    {
        [field: SerializeField] public bool HasStartTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnEnd { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnStart { get; private set; }= false;
        
        public override IEnumerator AddPassive(T entity, PassiveInstance<T> instance)
        {
            if (HasStartTurnEffect) entity.OnTurnStart += TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd += TurnEndEffect;
            
            return base.AddPassive(entity, instance);
        }
        
        public override IEnumerator RemovePassive(T entity, PassiveInstance<T> instance)
        {
            if (HasStartTurnEffect) entity.OnTurnStart -= TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd -= TurnEndEffect;
            
            return base.RemovePassive(entity, instance);
        }

        private IEnumerator TurnStartEffect(IBattleEntity entity)
        {
            if(entity is T battleEntity) return OnUnitTurnStartEffect(battleEntity);
            return OnTurnStartEffect(entity);
        }

        private IEnumerator TurnEndEffect(IBattleEntity entity)
        {
            if(entity is T battleEntity) return OnUnitTurnEndEffect(battleEntity);
            return OnTurnEndEffect(entity);
        }
        
        protected virtual IEnumerator OnTurnEndEffect(IBattleEntity battleEntity) { yield break; }
        protected virtual IEnumerator OnTurnStartEffect(IBattleEntity battleEntity) { yield break; }
        protected abstract IEnumerator OnUnitTurnEndEffect(T battleEntity);
        protected abstract IEnumerator OnUnitTurnStartEffect(T battleEntity);
    }
}

namespace Battle
{
    using ScriptableObjects;

    public interface IPassivesContainer<T> where T : MonoBehaviour,IPassivesContainer<T>
    {
        public delegate IEnumerator PassiveInstanceDelegate(PassiveInstance<T> passiveInstance);
        public void AddOnPassiveAddedCallback(PassiveInstanceDelegate callback); 
        public void AddOnPassiveRemovedCallback(PassiveInstanceDelegate callback);
        public void RemoveOnPassiveAddedCallback(PassiveInstanceDelegate callback); 
        public void RemoveOnPassiveRemovedCallback(PassiveInstanceDelegate callback); 
        public TPassiveInstance GetPassiveInstance<TPassiveInstance>(PassiveSO<T> passiveSo) where TPassiveInstance : PassiveInstance<T>;
        public IEnumerator AddPassiveEffect(PassiveSO<T> passiveSo, int amount = 1);
        public IEnumerator RemovePassiveEffect(PassiveSO<T> passiveSo);
        public IEnumerator RemovePassiveEffect(PassiveInstance<T> passiveInstance);
        public int GetPassiveEffectCount(Func<PassiveInstance<T>, bool> condition, out PassiveInstance<T> firstPassiveInstance);
    }
    
    public class PassiveInstance<T> where T : MonoBehaviour, IPassivesContainer<T>
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
            
            //Debug.Log($"Adding passive {SO.Name} to {container}");
            
            return SO.AddPassive(container,this);
        }
        
        public IEnumerator RemovePassive(T container)
        {
            Debug.Log($"Removing passive {SO.Name} from {container}");
            
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
}