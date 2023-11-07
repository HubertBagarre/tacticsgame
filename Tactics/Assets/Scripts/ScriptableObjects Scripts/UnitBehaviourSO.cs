using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.ActionSystem;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    using AbilityEvents;
    
    public abstract class UnitBehaviourSO : ScriptableObject
    {
        protected Unit AssociatedUnit { get; private set; } 
        protected bool IsInterrupted{ get; private set; } 
        protected bool IsCastingAbility { get; private set; }
        private bool isUnitTurn;
        protected Func<bool> endBehaviourCondition;
        
        // TODO - Remove if useless
        protected static BattleManager battleM;
        protected static TileManager tileM;
        protected static UnitManager unitM;
        protected static AbilityManager abilityM;
        
        // See UnitAbilitySO if can't be overriden (use a virtual bool func(Unit,Tile) instead)
        public virtual Func<Tile, bool> WalkableTileSelector {  get; protected set; } = tile => tile.IsWalkable && !tile.HasUnit();

        public static void SetBattleManager(BattleManager battleManager)
        {
            battleM = battleManager;
        }

        public static void SetTileManager(TileManager tileManager)
        {
            tileM = tileManager;
        }

        public static void SetUnitManager(UnitManager unitManager)
        {
            unitM = unitManager;
        }
        
        public static void SetAbilityManager(AbilityManager abilityManager)
        {
            abilityM = abilityManager;
        }

        public void InitBehaviour(Unit unit)
        {
            AssociatedUnit = unit;
            IsInterrupted = false;
            IsCastingAbility = false;
            endBehaviourCondition = () => true;
            
            isUnitTurn = false;
            
            EventManager.AddListener<StartAbilityCastEvent>(EnterCastingAbility);
            EventManager.AddListener<EndAbilityCastEvent>(ExitCastingAbility);
            
            InitBehaviourEffect(unit);
        }
        protected abstract void InitBehaviourEffect(Unit unit);
        public abstract void ShowBehaviourPreview(); // when you hover on timeline
        
        public IEnumerator RunBehaviour()
        {   
            IsInterrupted = false;
            IsCastingAbility = false;
            
            isUnitTurn = true;
            
            if(AssociatedUnit == null) yield break;
         
            yield return AssociatedUnit.StartCoroutine(RunBehaviourEffect());
            
            yield return new WaitUntil(endBehaviourCondition);
            
            Debug.Log("Ending Behaviour");
        }

        public CustomBattleAction UnitTurnBehaviourAction(NewUnit unit)
        {
            return new CustomBattleAction(Action,1f);
            
            void Action()
            {
                UnitTurnBattleAction(unit);
            }
        }

        protected abstract void UnitTurnBattleAction(NewUnit unit);

        
        protected abstract IEnumerator RunBehaviourEffect();

        public void OnBehaviourInterrupted()
        {
            IsInterrupted = true;
            OnBehaviourInterruptedEffect();
        } 
        protected abstract void OnBehaviourInterruptedEffect(); // returns true if interrupt behaviour this frame
        
        private void EnterCastingAbility(StartAbilityCastEvent ctx)
        {
            if(ctx.Caster != AssociatedUnit) return;
            IsCastingAbility = true;
        }
        
        private void ExitCastingAbility(EndAbilityCastEvent ctx)
        {
            if(ctx.Caster != AssociatedUnit) return;
            IsCastingAbility = false;
        }
    }
}
