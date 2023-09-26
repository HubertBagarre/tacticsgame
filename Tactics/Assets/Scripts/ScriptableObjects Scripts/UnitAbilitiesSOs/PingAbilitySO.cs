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
        [SerializeField] private List<UnitPassiveSO> passivesToRemove = new (); //not used

        public override string ConvertDescriptionLinks(Unit caster, string linkKey)
        {
            var split = linkKey.Split(":");

            switch (split[0])
            {
                case "ring":
                    int.TryParse(split[1],out var ring);

                    var tiles = 0;
                    for (int i = 0; i < ring+1; i++)
                    {
                        tiles += 4 * (2*i-1) + 4;
                    }
                    
                    return $"The {tiles} tiles surrounding the caster's tile"; //TODO - make generic ring shower (actually a shape shower)
                case "passive":
                    int.TryParse(split[1],out var passiveIndex);
                    var passive = passivesToAdd[passiveIndex];

                    return passive.Description;
            }
            
            return base.ConvertDescriptionLinks(caster, linkKey);
        }

        public override string ConvertedDescription(Unit caster)
        {
            var text = $"Select <color=green>1 enemy within <u><link=\"ring:{range}\">{range} rings</link></u></color>.";

            var passivesCount = passivesToAdd.Count;
            var hasPassives = passivesCount > 0;

            if (damage > 0)
            {
                text += $" Deal <color=orange>{damage} damage</color>";
                if (hasPassives) text += ".";
            }

            if (hasPassives)
            {
                text += " Inflicts";
                
                var passive = passivesToAdd[0];
                    
                text += $"<color=yellow>{(passive.IsStackable ? " 1 stack of ":"")} <u><link=\"passive:{0}\">{passive.Name}</link></u></color>";

                if (passivesCount >= 2)
                {
                    for (int i = 1; i < passivesCount; i++)
                    {
                        passive = passivesToAdd[i];

                        text += i == passivesCount - 1 ? " and" : ",";
                        text += $"<color=yellow>{(passive.IsStackable ? " 1 stack of ":"")} <u><link=\"passive:{i}\">{passive.Name}</link></u></color>";
                    }
                }
            }

            text += ".";
            
            return text;
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