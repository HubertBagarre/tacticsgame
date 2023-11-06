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

        public virtual void AddPassive(IPassivesContainer container, PassiveInstance instance)
        {
            OnAddedEffect(container, instance);
        }

        public virtual void RemovePassive(IPassivesContainer container, PassiveInstance instance)
        {
            OnRemovedEffect(container, instance);
        }
        
        protected abstract void OnAddedEffect(IPassivesContainer container, PassiveInstance instance);
        protected abstract void OnRemovedEffect(IPassivesContainer container, PassiveInstance instance);

        public PassiveInstance CreateInstance(int stacks = 1)
        {
            return new PassiveInstance(this,stacks);
        }
    }

    public abstract class RoundPassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasStartRoundEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndRoundEffect { get; private set; } = false;
        
        public override void AddPassive(IPassivesContainer container, PassiveInstance instance)
        {
            //if(HasStartRoundEffect) BattleManager.AddStartRoundPassive(RoundStartEffect(container, instance));
            //if(HasEndRoundEffect) BattleManager.AddEndRoundPassive(RoundEndEffect(container, instance));
            
            base.AddPassive(container, instance);
        }

        public override void RemovePassive(IPassivesContainer container, PassiveInstance instance)
        {
            //if(HasStartRoundEffect) BattleManager.RemoveStartRoundPassive(RoundStartEffect(container, instance));
            //if(HasEndRoundEffect) BattleManager.RemoveEndRoundPassive(RoundEndEffect(container, instance));
            
            base.RemovePassive(container, instance);
        }

        protected abstract void RoundStartEffect(IPassivesContainer container, PassiveInstance instance);
        protected abstract void RoundEndEffect(IPassivesContainer container, PassiveInstance instance);
    }

    public abstract class TilePassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasUnitEnterEffect { get; private set; } = false;
        [field: SerializeField] public bool HasUnitExitEffect { get; private set; } = false;
        
        public override IPassivesContainer GetContainer(NewTile tile) => tile;

        public override void AddPassive(IPassivesContainer container, PassiveInstance instance)
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
        protected abstract void UnitExitEffect(Tile tile, PassiveInstance instance);
    }
    
    public abstract class UnitPassiveSo : PassiveSO
    {
        [field: SerializeField] public bool HasStartTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool HasEndTurnEffect { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnEnd { get; private set; } = false;
        [field: SerializeField] public bool RemoveOnTurnStart { get; private set; }= false;
        
        public override IPassivesContainer GetContainer(NewTile tile) => tile.Unit;
        
        public override void AddPassive(IPassivesContainer container, PassiveInstance instance)
        {
            /*
            if (HasStartTurnEffect) entity.OnTurnStart += TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd += TurnEndEffect;
            */
            base.AddPassive(container, instance);
        }
        
        public override void RemovePassive(IPassivesContainer container, PassiveInstance instance)
        {
            /*
            if (HasStartTurnEffect) entity.OnTurnStart -= TurnStartEffect;
            if (HasEndTurnEffect) entity.OnTurnEnd -= TurnEndEffect;
            */
            base.RemovePassive(container, instance);
        }

        private void TurnStartEffect(TimelineEntity entity) => OnTurnStartEffect(entity);

        private void TurnEndEffect(TimelineEntity entity) => OnTurnEndEffect(entity);
        
        protected virtual void OnTurnEndEffect(TimelineEntity entity) {}
        protected virtual void OnTurnStartEffect(TimelineEntity entity) {}
    }
}

namespace Battle
{
    using ScriptableObjects;
    using ActionSystem;
    
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
        public int GetPassiveEffectCount(Func<PassiveInstance, bool> condition, out PassiveInstance firstPassiveInstance);
    }
    
    public class PassiveInstance
    {
        public PassiveSO SO { get; private set; }
        public bool IsStackable => SO.IsStackable;
        public int CurrentStacks { get; private set; } = 0;
        public bool HasMoreStacksThanMax => SO.MaxStacks != 0 && CurrentStacks >= SO.MaxStacks;

        private IPassivesContainer associatedPassiveContainer;
        public event Action<int> OnCurrentStacksChanged;
        public Dictionary<string,object> Data { get; private set; }

        public PassiveInstance(PassiveSO so, int startingStacks = 1)
        {
            SO = so;
            CurrentStacks = startingStacks - 1;
            Data = new Dictionary<string, object>();
        }
        
        public void OnPassiveAdded(IPassivesContainer container,int amount = 1)
        {
            if(associatedPassiveContainer == null) associatedPassiveContainer = container;
            
            if (HasMoreStacksThanMax) return;
            CurrentStacks += amount;
            if (HasMoreStacksThanMax) CurrentStacks = SO.MaxStacks;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            
            //Debug.Log($"Adding passive {SO.Name} to {container}");
            
            SO.AddPassive(container,this);
        }
        
        public void OnPassiveRemoved(IPassivesContainer container)
        {
            Debug.Log($"Removing passive {SO.Name} from {container}");
            
            SO.RemovePassive(container,this);
        }
        
        public void IncreaseStacks(int amount)
        {
            OnPassiveAdded(associatedPassiveContainer,amount);
        }

        public void DecreaseStacks(int amount)
        {
            if (CurrentStacks <= 1)
            {
                associatedPassiveContainer.RemovePassiveInstance(this);
                return;
            }
            
            CurrentStacks -= amount;
            
            if(CurrentStacks <= 0) CurrentStacks = 0;
            OnCurrentStacksChanged?.Invoke(CurrentStacks);
            
            OnPassiveRemoved(associatedPassiveContainer);
        }
    }

    public class AddPassiveBattleAction : BattleAction
    {
        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        public IPassivesContainer Container { get;}
        public PassiveSO PassiveSo { get;}
        public int Amount { get; private set; }
        public PassiveInstance PassiveInstance { get; private set; }
        
        public AddPassiveBattleAction(IPassivesContainer container, PassiveSO passive,int amount = 1)
        {
            YieldInstruction = null;
            CustomYieldInstruction = null;

            Container = container;
            PassiveSo = passive;
            Amount = amount;
        }
        
        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<AddPassiveBattleAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<AddPassiveBattleAction>(this));
        }

        protected override void AssignedActionPreWait()
        {
            if (!PassiveSo.IsStackable || (PassiveSo.IsStackable && Amount <= 0)) Amount = 1;
            
            PassiveInstance = Container.GetPassiveInstance(PassiveSo);
            
            if (PassiveInstance == null || !PassiveSo.IsStackable)
            {
                PassiveInstance = PassiveSo.CreateInstance(Amount);
            }
            
            // should get added right after
            //passiveInstances.Add(passiveInstance);
            
            PassiveInstance.OnPassiveAdded(Container,Amount);
        }

        protected override void AssignedActionPostWait()
        {
            
        }
    }
    
    public class RemovePassiveBattleAction : BattleAction
    {
        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        public IPassivesContainer Container { get;}
        public PassiveInstance PassiveInstance { get; private set; }
        
        public RemovePassiveBattleAction(IPassivesContainer container, PassiveInstance passiveInstance)
        {
            YieldInstruction = null;
            CustomYieldInstruction = null;

            Container = container;
            PassiveInstance = passiveInstance;
        }
        
        protected override void StartActionEvent()
        {
            EventManager.Trigger(new StartBattleAction<RemovePassiveBattleAction>(this));
        }

        protected override void EndActionEvent()
        {
            EventManager.Trigger(new EndBattleAction<RemovePassiveBattleAction>(this));
        }

        protected override void AssignedActionPreWait()
        {
            PassiveInstance.OnPassiveRemoved(Container);
        }

        protected override void AssignedActionPostWait()
        {
            
        }
    }
}