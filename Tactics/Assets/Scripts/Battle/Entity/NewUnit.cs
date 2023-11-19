using System;
using System.Collections.Generic;
using System.Linq;
using Battle.ActionSystem;
using UnityEngine;

namespace Battle
{
    using ScriptableObjects;
    
    [Serializable]
    public class NewUnit : TimelineEntity, IPassivesContainer
    {
        public UnitStatsInstance Stats { get; private set; }
        public UnitSO UnitSo => Stats.So;
        public Tile Tile { get; private set; }
        
        private List<PassiveInstance> passiveInstances;
        
        private List<UnitAbilityInstance> abilityInstances;
        
        public NewUnit(UnitSO so,Tile tile) : base(so.BaseSpeed, so.Initiative, so.Name)
        {
            Stats = so.CreateInstance();
            Tile = tile;
            
            passiveInstances = new List<PassiveInstance>();
            abilityInstances = new List<UnitAbilityInstance>();
            
            EventManager.AddListener<AddPassiveBattleAction>(AddPassiveInstanceToList);
        }

        private void AddPassiveInstanceToList(AddPassiveBattleAction ctx)
        {
            if(ctx.Container != this || ctx.PassiveInstance != null) return;
            
            passiveInstances.Add(ctx.PassiveInstance);
        }
        
        protected override void AddedToTimelineEffect()
        {
            return; // TODO fix this
            
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
            new AddPassiveBattleAction(this,passiveSo,amount).TryStack();
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
            
            passiveInstances.Remove(passiveInstance);
            new RemovePassiveBattleAction(this,passiveInstance).TryStack();
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
        protected override bool AutoAdvance => !isPlayerTurn;
        
        public NewUnit Unit { get; }
        public UnitStatsInstance Stats => Unit.Stats;
        
        private bool isPlayerTurn;
        
        public UnitTurnBattleAction(NewUnit unit)
        {
            Unit = unit;
        }
        
        protected override void Main()
        {
            Debug.Log($"Starting {Unit.Name} UnitTurn Battle Action");

            isPlayerTurn = Stats.Behaviour == null;

            if (isPlayerTurn)
            {
                Debug.Log($"No behaviour for {Unit.Name}, player turn");
                
                // probably send callback to enable ui;
                
                return;
            }
            
            Debug.Log($"Behaviour : {Stats.Behaviour}");
            
            var enumerable = Stats.Behaviour.UnitTurnBehaviourActions(Unit);
            
            if (enumerable == null)  return;
            
            foreach (var behaviourAction in enumerable)
            {
                EnqueueYieldedActions(behaviourAction);
            }
        }
    }
}


