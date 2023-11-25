using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    public enum PassiveType
    {
        Kit,
        Positive,
        Negative,
        Neutral
    }

    public abstract class PassiveSO : ScriptableObject
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

        public abstract IPassivesContainer GetContainer(NewTile tile);

        public virtual bool CanAddPassive(IPassivesContainer container,int amount,out PassiveInstance instance)
        {
            instance = container.GetPassiveInstance(this);

            if (instance == null || !IsStackable)
            {
                instance = CreateInstance(container, amount);
            }
            
            return instance != null;
        }

        public virtual bool CanAddStacks(PassiveInstance instance, int amount, out int newAmount)
        {
            newAmount = 0;
            if (!IsStackable)
            {
                Debug.LogWarning($"Trying to add stacks to {instance}, which is not stackable");
                return false;
            }
            
            newAmount = amount;
            if (MaxStacks == 0) return true;

            var currentStacks = instance.CurrentStacks;

            if (currentStacks + amount <= MaxStacks) return true;

            newAmount = MaxStacks - currentStacks;

            return newAmount != 0;
        }

        public virtual bool CanRemoveStacks(PassiveInstance instance, int amount, out int newAmount)
        {
            newAmount = amount;

            var currentStacks = instance.CurrentStacks;

            if (currentStacks <= 0) return false;

            if (amount > currentStacks) newAmount = currentStacks;

            return true;
        }

        public virtual bool CanRemovePassive(IPassivesContainer container)
        {
            if (IsUnmovable) return false;
            if(Type == PassiveType.Kit) return false;
            return true;
        }

        public virtual void AddPassive(PassiveInstance instance,int startingStacks)
        {
            OnAddedEffect(instance,startingStacks);
            instance.AddStacks(startingStacks);
        }

        public virtual void AddStacks(PassiveInstance instance, int amount = 1)
        {
            OnStacksAddedEffect(instance, amount);
        }

        public virtual void RemoveStacks(PassiveInstance instance, int amount = 1)
        {
            OnStacksRemovedEffect(instance, amount);
        }

        public virtual void RemovePassive(PassiveInstance instance)
        {
            OnRemovedEffect(instance);
        }

        protected abstract void OnAddedEffect(PassiveInstance instance,int startingStacks);
        protected abstract void OnStacksAddedEffect(PassiveInstance instance, int amount);

        protected abstract void OnStacksRemovedEffect(PassiveInstance instance, int amount);

        protected abstract void OnRemovedEffect(PassiveInstance instance);

        public PassiveInstance CreateInstance(IPassivesContainer container, int stacks = 1)
        {
            return new PassiveInstance(this, container, stacks);
        }
    }

    public abstract class RoundPassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasStartRoundEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndRoundEffect { get; private set; } = false;

        public override void AddPassive(PassiveInstance instance,int startingStacks)
        {
            //if(HasStartRoundEffect) BattleManager.AddStartRoundPassive(RoundStartEffect(container, instance));
            //if(HasEndRoundEffect) BattleManager.AddEndRoundPassive(RoundEndEffect(container, instance));

            base.AddPassive(instance,startingStacks);
        }

        public override void RemovePassive(PassiveInstance instance)
        {
            //if(HasStartRoundEffect) BattleManager.RemoveStartRoundPassive(RoundStartEffect(container, instance));
            //if(HasEndRoundEffect) BattleManager.RemoveEndRoundPassive(RoundEndEffect(container, instance));

            base.RemovePassive(instance);
        }

        protected abstract void RoundStartEffect(PassiveInstance instance);
        protected abstract void RoundEndEffect(PassiveInstance instance);
    }

    public abstract class TilePassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasUnitEnterEffect { get; private set; } = false;
        [field: SerializeField] public bool HasUnitExitEffect { get; private set; } = false;

        public override IPassivesContainer GetContainer(NewTile tile) => tile;

        /*
        public override void AddPassive(PassiveInstance instance)
        {
            
            if (container is not NewTile tile)
            {
                base.AddPassive(container, instance);
                return;
            }

            // TODO - now can use events
            //if(HasUnitEnterEffect) tile.AddOnUnitEnterEvent(UnitEnterEffect(tile, instance));
            //if(HasUnitExitEffect) tile.AddOnUnitExitEvent(UnitExitEffect(tile, instance));

            base.AddPassive(container, instance);
        }

        public override void RemovePassive(IPassivesContainer container, PassiveInstance instance)
        {
            if (container is not NewTile tile)
            {
                base.AddPassive(container, instance);
                return;
            }

            //if(HasUnitEnterEffect) tile.RemoveOnUnitEnterEvent(UnitEnterEffect(tile, instance));
            //if(HasUnitExitEffect) tile.RemoveOnUnitExitEvent(UnitExitEffect(tile, instance));

            base.RemovePassive(tile, instance);
        }

        protected abstract void UnitEnterEffect(Tile tile, PassiveInstance instance);
        protected abstract void UnitExitEffect(Tile tile, PassiveInstance instance);*/
    }

    public abstract class UnitPassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasStartTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnEnd { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnStart { get; private set; } = false;

        public override IPassivesContainer GetContainer(NewTile tile) => tile.Unit;
/*
        public override void AddPassive(IPassivesContainer container, PassiveInstance instance)
        {
        
            if (HasStartTurnEffect) entity.OnTurnStart += TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd += TurnEndEffect;
        
            base.AddPassive(container, instance);
        }

        public override void RemovePassive(IPassivesContainer container, PassiveInstance instance)
        {
            
            if (HasStartTurnEffect) entity.OnTurnStart -= TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd -= TurnEndEffect;
            
            base.RemovePassive(container, instance);
        }

        private void TurnStartEffect(TimelineEntity entity) => OnTurnStartEffect(entity);

        private void TurnEndEffect(TimelineEntity entity) => OnTurnEndEffect(entity);

        protected virtual void OnTurnEndEffect(TimelineEntity entity)
        {
        }

        protected virtual void OnTurnStartEffect(TimelineEntity entity)
        {
        }*/
    }
}

namespace Battle
{
    using ScriptableObjects;

    public interface IPassivesContainer
    {
        /*
        public delegate IEnumerator PassiveInstanceDelegate(PassiveInstance passiveInstance);
        public void AddOnPassiveAddedCallback(PassiveInstanceDelegate callback);
        public void AddOnPassiveRemovedCallback(PassiveInstanceDelegate callback);
        public void RemoveOnPassiveAddedCallback(PassiveInstanceDelegate callback);
        public void RemoveOnPassiveRemovedCallback(PassiveInstanceDelegate callback);
        */

        public PassiveInstance GetPassiveInstance(PassiveSO passiveSo);
        public void AddPassiveEffect(PassiveSO passiveSo, int amount = 1);
        public void RemovePassive(PassiveSO passiveSo);
        public void RemovePassiveInstance(PassiveInstance passiveInstance);

        public int GetPassiveEffectCount(Func<PassiveInstance, bool> condition,
            out PassiveInstance firstPassiveInstance);
    }

    public class PassiveInstance
    {
        public PassiveSO SO { get; private set; }
        public bool IsStackable => SO.IsStackable;
        private int currentStacks = 0;
        public int CurrentStacks { get => currentStacks
            ;
            private set
            {
                currentStacks = value;
                OnCurrentStacksChanged?.Invoke(CurrentStacks);
            } 
        }

        public IPassivesContainer Container { get; }
        public event Action<int> OnCurrentStacksChanged;
        public Dictionary<string, object> Data { get; private set; }

        public PassiveInstance(PassiveSO so, IPassivesContainer container, int startingStacks)
        {
            SO = so;
            Container = container;
            CurrentStacks = startingStacks - 1;
            Data = new Dictionary<string, object>();
        }
        
        public void AddStacks(int amount)
        {
            var canAddStacks = SO.CanAddStacks(this, amount, out var newAmount);
            
            if(!canAddStacks) return;
            
            var addStacksAction = new AddStacksBattleAction(this,newAmount);
            
            addStacksAction.TryStack();
        }

        public void RemoveStacks(int amount)
        {
            var canRemoveStacks = SO.CanRemoveStacks(this, amount, out var newAmount);
            
            if(!canRemoveStacks) return;
            
            var removeStacksAction = new RemoveStacksBattleAction(this,newAmount);
            
            removeStacksAction.TryStack();
        }
        

        public abstract class PassiveInstanceBattleAction : SimpleStackableAction
        {
            protected override YieldInstruction YieldInstruction { get; }
            protected override CustomYieldInstruction CustomYieldInstruction { get; }
            public int Amount { get; private set; }
            public PassiveInstance PassiveInstance { get; }
            
            protected PassiveInstanceBattleAction(PassiveInstance passiveInstance, int amount = 1)
            {
                YieldInstruction = null;
                CustomYieldInstruction = null;
                
                PassiveInstance = passiveInstance;
                Amount = amount;
                
                if(passiveInstance == null) Debug.LogWarning("passive instance is null");
            }
        }
        
        public class AddPassiveBattleAction : PassiveInstanceBattleAction
        {
            public AddPassiveBattleAction(PassiveInstance passiveInstance, int amount = 1) : base(passiveInstance, amount)
            {
                
            }
            
            protected override void Main()
            {
                PassiveInstance.SO.AddPassive(PassiveInstance,Amount);
            }
        }

        public class AddStacksBattleAction : PassiveInstanceBattleAction
        {
            public AddStacksBattleAction(PassiveInstance passiveInstance, int amount = 1) : base(passiveInstance, amount)
            {
            }
            
            protected override void Main()
            {
                PassiveInstance.SO.AddStacks(PassiveInstance,Amount);
            }
        }
        
        public class RemoveStacksBattleAction : PassiveInstanceBattleAction
        {
            public RemoveStacksBattleAction(PassiveInstance passiveInstance, int amount = 1) : base(passiveInstance, amount)
            {
            }
            
            protected override void Main()
            {
                PassiveInstance.SO.RemoveStacks(PassiveInstance,Amount);
            }
        }

        public class RemovePassiveBattleAction : PassiveInstanceBattleAction
        {
            public RemovePassiveBattleAction(PassiveInstance passiveInstance, int _ = 1) : base(passiveInstance, _)
            {
            }
            
            protected override void Main()
            {
                PassiveInstance.SO.RemovePassive(PassiveInstance);
            }

            
        }
    }
}