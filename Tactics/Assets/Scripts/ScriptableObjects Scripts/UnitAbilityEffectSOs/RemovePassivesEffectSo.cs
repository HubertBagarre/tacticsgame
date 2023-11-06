using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Remove Passives")]
    public class RemovePassivesEffectSo : UnitAbilityEffectSO
    {
        [SerializeField] private List<PassiveSO> passivesToRemove = new();

        public override string ConvertedDescription(Unit caster)
        {
            var text = " Inflicts";

            var passive = passivesToRemove[0];
            var passivesCount = passivesToRemove.Count;

            text +=
                $"<color=yellow>{(passive.IsStackable ? " 1 stack of " : "")} <u><link=\"passive:{0}\">{passive.Name}</link></u></color>";

            if (passivesCount >= 2)
            {
                for (int i = 1; i < passivesCount; i++)
                {
                    passive = passivesToRemove[i];

                    text += i == passivesCount - 1 ? " and" : ",";
                    text +=
                        $"<color=yellow>{(passive.IsStackable ? " 1 stack of " : "")} <u><link=\"passive:{i}\">{passive.Name}</link></u></color>";
                }
            }

            text += ".";

            return text;
        }

        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            foreach (var tile in targetTiles)
            {
                Debug.Log($"Removing Passive on {tile}");

                //play animation
                yield return null;

                if(!tile.HasUnit()) continue;
                
                var target = tile.Unit;
            
                foreach (var passive in passivesToRemove)
                {
                    target.RemovePassiveEffect(passive);
                }
            }
            
            yield return null;
        }
    }
}