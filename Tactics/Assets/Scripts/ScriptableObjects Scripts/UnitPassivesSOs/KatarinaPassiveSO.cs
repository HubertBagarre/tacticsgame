using System;
using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(fileName = "KatarinaPassive", menuName = "Battle Scriptables/Unique Passive/Katarina Passive")]
    public class KatarinaPassiveSO : PassiveSO
    {
        [SerializeField] private PassiveSO daggerPassive;
        [SerializeField] private PassiveSO tileDaggerPassive;
        
        public override IPassivesContainer GetContainer(NewTile tile)
        {
            return tile.Unit;
        }

        protected override void OnAddedEffect(PassiveInstance instance, int startingStacks)
        {
            Debug.Log("Here");
        }

        protected override void OnStacksAddedEffect(PassiveInstance instance, int amount)
        {
            
        }

        protected override void OnStacksRemovedEffect(PassiveInstance instance, int amount)
        {
            
        }

        protected override void OnRemovedEffect(PassiveInstance instance)
        {
            
        }

        /*
        protected override void OnAddedEffect(IPassivesContainer container, PassiveInstance instance)
        {
            EventManager.AddListener<AddPassiveBattleAction>(EffectOnDaggerAdded,true);

            EventManager.EventCallback<AddPassiveBattleAction> action = EffectOnDaggerAdded;
            instance.Data.Add("EffectOnDaggerAdded",action);
            
            // TODO - Tile Enter event
            //container.OnTileEnter += AddDaggerOnThrownDaggerPickup;
            
            return;
            void EffectOnDaggerAdded(AddPassiveBattleAction ctx)
            {
                if(ctx.Container != container) return;
                if(ctx.PassiveSo != daggerPassive) return;
                
                Debug.Log("Spin");
            }
        }
        
        private void AddDaggerOnThrownDaggerPickup(NewUnit unit,NewTile tile)
        {
            if (tile?.GetPassiveInstance(tileDaggerPassive) == null) return;

            unit.AddPassiveEffect(daggerPassive);
        }

        protected override void OnRemovedEffect(IPassivesContainer container, PassiveInstance instance)
        {
            if(instance.Data.TryGetValue("EffectOnDaggerAdded",out var action))
            {
                EventManager.RemoveListener<AddPassiveBattleAction>(action as EventManager.EventCallback<AddPassiveBattleAction>);
            }
            
            //unit.OnTileEnter -= AddDaggerOnThrownDaggerPickup;
        }*/
    }
}

