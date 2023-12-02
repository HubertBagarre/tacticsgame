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
        [field: Header("Current Stats")]
        
        [field: SerializeField] public UnitStatsInstance Stats { get; private set; }
        public UnitSO SO => Stats.So;
        
        public bool UsePlayerBehaviour { get; private set; }
        public NewTile Tile { get; private set; }
        
        private List<PassiveInstance> passiveInstances;
        public IReadOnlyList<PassiveInstance> PassiveInstances => passiveInstances;
        
        private List<AbilityInstance> abilityInstances;
        public IReadOnlyList<AbilityInstance> AbilityInstances => abilityInstances;
        
        [SerializeField] private int currentUltimatePoints;
        public int CurrentUltimatePoints
        {
            get => currentUltimatePoints;
            protected set
            {
                var previous = currentUltimatePoints;
                currentUltimatePoints = value;
                OnUltimatePointsAmountChanged?.Invoke(previous,currentUltimatePoints);
            }
        }
        public event Action<int, int> OnUltimatePointsAmountChanged;

        public NewUnit(UnitSO so,NewTile tile,bool usePlayerBehaviour = false) : base(so.BaseSpeed, so.Initiative, so.Name)
        {
            Stats = so.CreateInstance();
            
            Tile = tile;
            UsePlayerBehaviour = usePlayerBehaviour;
            
            passiveInstances = new List<PassiveInstance>();
            abilityInstances = new List<AbilityInstance>();

            foreach (var abilityToAdd in so.Abilities)
            {
                abilityInstances.Add(abilityToAdd.CreateInstance());
            }
        }
        
        #region DebugContextMenu
#if UNITY_EDITOR
        
        public void DebugPassives()
        {
            Debug.Log($"Found {passiveInstances.Count} passives instances on {Name} :");
            foreach (var passiveInstance in passiveInstances)
            {
                var text = $"{passiveInstance.SO.Name} - {passiveInstance.CurrentStacks}";
                Debug.Log($"{text}");
            }
        }
        
#endif
        #endregion

        public void SetTeam(int team)
        {
            Team = team;
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
            foreach (var passiveToAdd in SO.StartingPassives)
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

        public void AddPassiveEffect(PassiveSO passiveSo, int amount = 1, bool force = false)
        {
            var addPassiveAction = new PassiveInstance.AddPassiveBattleAction(this,passiveSo,amount,force);
            
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

        public int GetPassiveInstancesCount(Func<PassiveInstance, bool> condition, out PassiveInstance firstPassiveInstance)
        {
            firstPassiveInstance = passiveInstances.FirstOrDefault(condition);
            
            return passiveInstances.Count(condition);
        }

        public override IEnumerable<StackableAction.YieldedAction> EntityTurnYieldedActions => new[] { new StackableAction.YieldedAction(new UnitTurnBattleAction(this).TryStack)};

        public override string ToString()
        {
            return SO != null ? $"Unit ({SO.name})" : base.ToString();
        }
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
            
            if (IsPlayerTurn) return;
            
            var enumerable = Stats.Behaviour.UnitTurnBehaviourActions(Unit,EnqueueYieldedActions);
            
            if (enumerable == null) return;
            
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


