using System.Collections;
using UnityEngine;

namespace Battle.ScriptableObjects.Passive
{
    [CreateAssetMenu(fileName = "KatarinaPassive", menuName = "Battle Scriptables/Unique Passive/Katarina Passive")]
    public class KatarinaPassiveSO : PassiveSO<Unit>
    {
        [SerializeField] private PassiveSO<Unit> daggerPassive;
        [SerializeField] private PassiveSO<Tile> tileDaggerPassive;
        
        protected override IEnumerator OnAddedEffect(Unit unit, PassiveInstance<Unit> instance)
        {
            unit.AddOnPassiveAddedCallback(EffectOnDaggerAdded);
            unit.OnTileEnter += AddDaggerOnThrownDaggerPickup;
            
            yield break;
        }

        private IEnumerator EffectOnDaggerAdded(PassiveInstance<Unit> instance)
        {
            if (instance.SO != daggerPassive) yield break;
            
            Debug.Log("Spin");
            
            //Damage here (spin)
        }
        
        private IEnumerator AddDaggerOnThrownDaggerPickup(Unit unit,Tile tile)
        {
            if(tile == null) yield break;
            if (tile.GetPassiveInstance<PassiveInstance<Tile>>(tileDaggerPassive) == null) yield break;

            yield return unit.StartCoroutine(unit.AddPassiveEffect(daggerPassive));
        }

        protected override IEnumerator OnRemovedEffect(Unit unit, PassiveInstance<Unit> instance)
        {
            unit.RemoveOnPassiveAddedCallback(EffectOnDaggerAdded);
            unit.OnTileEnter -= AddDaggerOnThrownDaggerPickup;
            yield break;
        }
    }
}

