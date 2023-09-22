using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Ability/Ping")]
    public class PingAbilitySO : UnitAbilitySO
    {
        [SerializeField] private int range = 2;
        [SerializeField] private int damage = 2;
        [SerializeField] private List<UnitPassiveSO> passivesToAdd = new();
        [SerializeField] private List<UnitPassiveSO> passivesToRemove = new ();

        public override string ConvertedDescription(Unit caster)
        {
            var text = description.Replace("%range%", $"{range}");
            
            //Add text if damage > 0
            //Deal <color=orange>%damage% damage</color> to it
            
            //Add text if passives to add > 0
            
            //Add text if passives to remove > 0
            
            //Inflict 1 stack of Burn to it.

            return text;
            
            return base.ConvertedDescription(caster);
        }

        protected override bool TileSelectionMethod(Unit caster, Tile selectableTile, List<Tile> currentlySelectedTiles)
        {
            return caster.Tile.IsInSurroundingTileDistance(selectableTile,range) && selectableTile.HasUnit();
        }

        protected override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            Debug.Log($"Ping on {targetTiles[0]}");
            
            //play animation
            yield return null;
            
            var target = targetTiles[0].Unit;
            target.TakeDamage(damage);
            if (passivesToAdd.Count > 0)
            {
                foreach (var passive in passivesToAdd)
                {
                    var routine = target.AddPassiveEffect(passive);
                    if(routine != null) yield return target.StartCoroutine(routine);
                }
            }
            if (passivesToRemove.Count > 0)
            {
                foreach (var passive in passivesToRemove)
                {
                    var routine = target.RemovePassiveEffect(passive);
                    if(routine != null) yield return target.StartCoroutine(routine);
                }
            }
            
            yield return null;
        }
    }
}