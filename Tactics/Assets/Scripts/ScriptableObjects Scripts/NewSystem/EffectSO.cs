using System;

namespace Battle.ScriptableObjects.Ability
{
    public abstract class EffectSO : ParametableSO
    {
        public void EffectFullParameters(NewUnit caster, NewTile[] targetTiles, string parameters)
        {
            Effect(caster, targetTiles, LocalGetParameterValue);

            return;

            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }

        protected abstract void Effect(NewUnit caster, NewTile[] targetTiles, Func<string, dynamic> parameterGetter);
    }

    public abstract class AbilityEffectSO : EffectSO
    {
        public string TextFullParameters(NewTile referenceTile,string parameters)
        {
            return Text(referenceTile,LocalGetParameterValue);
            
            dynamic LocalGetParameterValue(string parameter)
            {
                return GetParameterValue<dynamic>(parameter, GetSpecificParameter(parameters));
            }
        }
        
        /// <summary>
        /// text should be used in this context : "Select target,{Text()},{Text()} and {Text()}"
        /// </summary>
        protected abstract string Text(NewTile referenceTile,Func<string,dynamic> parameterGetter);
    }
}
