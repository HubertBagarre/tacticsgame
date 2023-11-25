using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    
    [Serializable]
    public class NewUnit : TimelineEntity, IPassivesContainer
    {
        public UnitStatsInstance Stats { get; private set; }
        public UnitSO UnitSo => Stats.So;
        
        public bool UsePlayerBehaviour { get; private set; }
        public Tile Tile { get; private set; }
        
        private List<PassiveInstance> passiveInstances;
        
        private List<UnitAbilityInstance> abilityInstances;
        
        public NewUnit(UnitSO so,Tile tile,bool usePlayerBehaviour = false) : base(so.BaseSpeed, so.Initiative, so.Name)
        {
            Stats = so.CreateInstance();
            Tile = tile;
            UsePlayerBehaviour = usePlayerBehaviour;
            
            passiveInstances = new List<PassiveInstance>();
            abilityInstances = new List<UnitAbilityInstance>();
        }
        
        public void SetUsePlayerBehaviour(bool usePlayerBehaviour)
        {
            UsePlayerBehaviour = usePlayerBehaviour;
        }

        public void AddPassiveInstanceToList(PassiveInstance passiveInstance)
        {
            if(passiveInstance == null || passiveInstances.Contains(passiveInstance)) return;
            
            passiveInstances.Add(passiveInstance);
        }

        public void RemovePassiveInstanceFromList(PassiveInstance passiveInstance)
        {
            if(passiveInstance == null || !passiveInstances.Contains(passiveInstance)) return;
            
            passiveInstances.Remove(passiveInstance);
        }
        
        protected override void AddedToTimelineEffect()
        {
            Debug.Log($"Adding {UnitSo.StartingPassives.Count} starting passives to {Name}");
            
            foreach (var passiveToAdd in UnitSo.StartingPassives)
            {
                passiveToAdd.AddPassiveToContainer(this);
            }
        }

        protected override void TurnStart()
        {
            Stats.CurrentMovement = Stats.Movement;
            
            foreach (var abilityInstance in abilityInstances)
            {
                abilityInstance.DecreaseCurrentCooldown(1);  //should be battle action/callbackable
            }
            
            // this should go in battle manager, end current entity turn
            UIBattleManager.OnEndTurnButtonClicked += EndTurn;

            var unitTurnAction = new UnitTurnBattleAction(this);
            
            Debug.Log($"Stacking unit turn action of {unitTurnAction.Unit}");
            
            unitTurnAction.TryStack();
            
            //TODO - Create new Unit Turn Battle Action;
            // enqueue all actions in the battle action (based on behaviour) 
            // enqueue end turn action
            // PLAYER HAS NO ACTIONS, SO NO ENQUEUE OF END TURN ACTION
            // run battle action
		    //bosses/ units with conditional behaviour should have a special unitturn that behaves like the roundaction and dynamicaly adds battleactions then repeats itself
        }

        protected override void TurnEnd()
        {
            UIBattleManager.OnEndTurnButtonClicked -= EndTurn;
        }
        
        public PassiveInstance GetPassiveInstance(PassiveSO passiveSo)
        {
            return passiveInstances.FirstOrDefault(passiveInstance => passiveInstance.SO == passiveSo);
        }

        public void AddPassiveEffect(PassiveSO passiveSo, int amount = 1)
        {
            var canCanPassive = passiveSo.CanAddPassive(this,amount,out var passiveInstance);
            
            if(!canCanPassive) return;
            
            if (passiveInstances.Contains(passiveInstance))
            {
                passiveInstance.AddStacks(amount);
                return;
            }
            
            var addPassiveAction = new PassiveInstance.AddPassiveBattleAction(passiveInstance,amount);
            
            addPassiveAction.TryStack();
        }

        public void RemovePassive(PassiveSO passiveSo)
        {
            var currentInstance = GetPassiveInstance(passiveSo);
            RemovePassiveInstance(currentInstance);
        }

        public void RemovePassiveInstance(PassiveInstance passiveInstance)
        {
            if(passiveInstance == null) return;
            if(!passiveInstances.Contains(passiveInstance)) return;
            
            var canRemovePassive = passiveInstance.SO.CanRemovePassive(this);
            
            if(!canRemovePassive) return;
            
            var removePassiveAction = new PassiveInstance.RemovePassiveBattleAction(passiveInstance);
            
            removePassiveAction.TryStack();
        }

        public int GetPassiveEffectCount(Func<PassiveInstance, bool> condition, out PassiveInstance firstPassiveInstance)
        {
            firstPassiveInstance = passiveInstances.FirstOrDefault(condition);
            
            return passiveInstances.Count(condition);
        }

        public override IEnumerable<StackableAction.YieldedAction> EntityTurnYieldedActions => new[] { new StackableAction.YieldedAction(new UnitTurnBattleAction(this).TryStack)};
    }
    
    public class UnitTurnBattleAction : SimpleStackableAction
    {
        protected override YieldInstruction YieldInstruction { get; }
        protected override CustomYieldInstruction CustomYieldInstruction { get; }
        protected override bool AutoAdvance => !IsPlayerTurn && advance;
        private bool advance = true;
        
        public NewUnit Unit { get; }
        public UnitStatsInstance Stats => Unit.Stats;
        public bool IsPlayerTurn => Stats.Behaviour == null || Unit.UsePlayerBehaviour;
        
        public event Action OnRequestEndTurn;
        
        public UnitTurnBattleAction(NewUnit unit)
        {
            Unit = unit;
            advance = true;
        }
        
        protected override void Main()
        {
            advance = true;
            
            Debug.Log($"Starting {Unit.Name} UnitTurn Battle Action");
            
            if (IsPlayerTurn)
            {
                Debug.Log($"No behaviour for {Unit.Name}, player turn");
                
                return;
            }
            
            Debug.Log($"Behaviour : {Stats.Behaviour}");
            
            var enumerable = Stats.Behaviour.UnitTurnBehaviourActions(Unit,EnqueueYieldedActions);
            
            if (enumerable == null)  return;
            
            foreach (var behaviourAction in enumerable)
            {
                EnqueueYieldedActions(behaviourAction);
            }
            
            if(!IsPlayerTurn) EnqueueYieldedActions(TryEndTurnYieldedAction());
        }
        
        private YieldedAction TryEndTurnYieldedAction()
        {
            return new YieldedAction(RequestEndTurnIfNoMoreYieldedActions);

            void RequestEndTurnIfNoMoreYieldedActions()
            {
                Debug.Log($"Trying to end {Unit}'s turn");

                var actionsLeft = GetYieldedActionsCount();
                
                Debug.Log($"Actions left: {actionsLeft}");

                advance = actionsLeft > 0;

                if (actionsLeft > 0)
                {
                    Debug.Log("Unit has more actions, queueing and go next");
                    EnqueueYieldedActions(TryEndTurnYieldedAction());
                    return;
                }
                
                Debug.Log("No more actions, asking requesting end turn");

                RequestEndTurn();
            }
        }

        public void RequestEndTurn()
        {
            OnRequestEndTurn?.Invoke();
        }
    }
}


