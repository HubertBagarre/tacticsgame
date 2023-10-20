using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Add Passives")]
    public class AddPassivesEffectSO : UnitAbilityEffectSO
    {
        [SerializeField] private List<UnitPassiveSO> passivesToAdd = new();

        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("passive:")) return false;
            
            var split = linkKey.Split(":");
            
            if(!int.TryParse(split[1],out var passiveIndex)) return false;
            
            var passive = passivesToAdd[passiveIndex];

            text = passive.Description;
            return true;
        }

        public override string ConvertedDescription(Unit caster)
        {
            var text = "Inflicts";
                
            var passive = passivesToAdd[0];
            var passivesCount = passivesToAdd.Count;
                    
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

            text += ".";
            
            return text;
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                Debug.Log($"Adding Passive on {tile}");

                //play animation
                yield return null;

                if(!tile.HasUnit()) continue;
                
                var target = tile.Unit;
            
                foreach (var passive in passivesToAdd)
                {
                    var routine = target.AddPassiveEffect(passive);
                    if (routine != null) yield return target.StartCoroutine(routine);
                }
            }
            
            yield return null;
        }
    }
}


