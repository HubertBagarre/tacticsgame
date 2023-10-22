using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.ScriptableObjects.Ability.Effect
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Ability Effect/Add Passives")]
    public class AddPassivesEffectSO : UnitAbilityEffectSO
    {
        [SerializeField] private List<PassiveToAdd> passivesToAdd = new();
        
        [SerializeField,Tooltip("Takes Neutral, Positive and Negative")] private List<PassiveType> order = 
            new(){PassiveType.Neutral,PassiveType.Positive,PassiveType.Negative};
        private Dictionary<PassiveType,IEnumerable<PassiveToAdd>> passivesByType = new(){ { PassiveType.Neutral,Enumerable.Empty<PassiveToAdd>()},{ PassiveType.Positive,Enumerable.Empty<PassiveToAdd>()},{ PassiveType.Negative,Enumerable.Empty<PassiveToAdd>()}};
        
        private IEnumerable<PassiveToAdd> NeutralPassives => passivesToAdd.Where(passiveToAdd => !passiveToAdd.IsType(PassiveType.Positive) && !passiveToAdd.IsType(PassiveType.Positive));
        private IEnumerable<PassiveToAdd> PositivePassives => passivesToAdd.Where(passiveToAdd => passiveToAdd.IsType(PassiveType.Positive));
        private IEnumerable<PassiveToAdd> NegativePassives => passivesToAdd.Where(passiveToAdd => passiveToAdd.IsType(PassiveType.Negative));

        public override bool ConvertDescriptionLinks(Unit caster, string linkKey, out string text)
        {
            text = string.Empty;
            if (!linkKey.Contains("passive:")) return false;
            
            var split = linkKey.Split(":");
            
            if(!int.TryParse(split[1],out var passiveIndex)) return false;
            
            var unitPassive = passivesToAdd[passiveIndex].UnitPassive;
            if (unitPassive != null)
            {
                text = unitPassive.Description;
                return true;
            }
            
            var tilePassive = passivesToAdd[passiveIndex].TilePassive;
            if (tilePassive == null) return false;
            text = tilePassive.Description;
            return true;

        }

        // TODO - Handle all PassiveToAdd fields
        public override string ConvertedDescription(Unit caster)
        {
            var text = string.Empty;
            
            RefreshDict();

            for (var index = 0; index < order.Count; index++)
            {
                var passiveType = order[index];
                var passiveText = GetText(passivesByType[passiveType], GetVerb(passiveType));
                if (passiveText != string.Empty) text += passiveText;
            }

            return text;

            string GetVerb(PassiveType passiveType)
            {
                return passiveType switch
                {
                    PassiveType.Positive => "Grants",
                    PassiveType.Negative => "Inflicts",
                    _ => "Applies"
                };
            }

            string GetText(IEnumerable<PassiveToAdd> enumerable,string enumerableText)
            {
                var list = enumerable.ToList();
                
                if (list.Count == 0) return string.Empty;
                
                var passivesCount = list.Count;
                
                var passiveToAdd = list[0];

                enumerableText += passiveToAdd.GetText();
                

                if (passivesCount >= 2)
                {
                    for (var i = 1; i < passivesCount; i++)
                    {
                        passiveToAdd = list[i];

                        enumerableText += i == passivesCount - 1 ? " and" : ",";
                        enumerableText += passiveToAdd.GetText();
                    }
                }

                enumerableText += ".\n";
                return enumerableText;
            }
        }
        
        public override IEnumerator AbilityEffect(Unit caster, Tile[] targetTiles)
        {
            RefreshDict();
            
            foreach (var tile in targetTiles)
            {
                //play animation
                yield return null;
                
                foreach (var passiveType in order)
                {
                    foreach (var passiveToAdd in passivesByType[passiveType])
                    {
                        yield return caster.StartCoroutine(passiveToAdd.AddPassive(tile));
                    }
                }
            }
        }
        
        private void RefreshDict()
        {
            if(!order.Contains(PassiveType.Neutral)) order.Add(PassiveType.Neutral);
            if(!order.Contains(PassiveType.Positive)) order.Add(PassiveType.Positive);
            if(!order.Contains(PassiveType.Negative)) order.Add(PassiveType.Negative);
            order = order.Distinct().ToList();
                
            passivesByType[PassiveType.Neutral] = NeutralPassives;
            passivesByType[PassiveType.Positive] = PositivePassives;
            passivesByType[PassiveType.Negative] = NegativePassives;
        }
    }
}


