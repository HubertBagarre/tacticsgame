using System;
using System.Collections.Generic;
using Battle;
using Battle.ScriptableObjects.Ability;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Scriptables/Effect/Debug")]
public class DebugEffectSO : EffectSO
{
    public override IEnumerable<string> SpecificParameters => new []{"Log"};
    protected override object InterpretParameter(string parameter, string value, bool wasNotFound)
    {
        if (wasNotFound) return string.Empty;
        
        return value;
    }

    protected override void Effect(NewUnit caster, NewTile[] targetTiles, Func<string, dynamic> parameterGetter)
    {
        var text = parameterGetter.Invoke("Log") as string;
        
        Debug.Log($"{text}");
    }
}
