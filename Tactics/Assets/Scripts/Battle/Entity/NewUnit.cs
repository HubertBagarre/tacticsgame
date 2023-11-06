using System;
using System.Collections.Generic;
using System.Linq;
using Battle.ActionSystem;

namespace Battle
{
    using ScriptableObjects;
    
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
                abilityInstance.DecreaseCurrentCooldown(1);
            }
            
            // this should go in battle manager, end current entity turn
            UIBattleManager.OnEndTurnButtonClicked += EndTurn;
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
            BattleAction.StartNewBattleAction(new AddPassiveBattleAction(this,passiveSo,amount));
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
            BattleAction.StartNewBattleAction(new RemovePassiveBattleAction(this,passiveInstance));
        }

        public int GetPassiveEffectCount(Func<PassiveInstance, bool> condition, out PassiveInstance firstPassiveInstance)
        {
            firstPassiveInstance = passiveInstances.FirstOrDefault(condition);
            
            return passiveInstances.Count(condition);
        }
    }
}


