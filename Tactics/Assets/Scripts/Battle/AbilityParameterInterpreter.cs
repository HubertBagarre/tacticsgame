using System.Collections.Generic;
using Battle;
using Battle.ScriptableObjects;
using Battle.ScriptableObjects.Ability;
using Battle.ScriptableObjects.Ability.Components;
using MoonSharp.Interpreter;
using Unity.VisualScripting;
using UnityEngine;

[MoonSharpUserData]
public class AbilityParameterInterpreter
{
    private NewUnit caster;
    private NewTile[] targetTiles;
    private List<ConditionalEffect<EffectSO>> availableConditionalEffects;
    private List<CustomizableAbilityCondition> availableConditions;
    private List<EffectsOnTarget<EffectSO>> availableEffects;
    public List<EffectsOnTarget<EffectSO>> SelectedEffects { get; private set; }

    public AbilityParameterInterpreter(NewUnit caster, NewTile[] targetTiles,
        List<ConditionalEffect<EffectSO>> availableConditionalEffects,
        List<CustomizableAbilityCondition> availableConditions,
        List<EffectsOnTarget<EffectSO>> availableEffects)
    {
        this.caster = caster;
        this.targetTiles = targetTiles;
        this.availableConditionalEffects = availableConditionalEffects;
        this.availableConditions = availableConditions;
        this.availableEffects = availableEffects;
        SelectedEffects = new List<EffectsOnTarget<EffectSO>>();
    }

    public void Log(string message)
    {
        Debug.Log($"Lua: {message}");
    }
    
    public bool CanCast(string id)
    {
        var index = availableConditions.FindIndex(condition => condition.Id == id);
        return CanCast(index);
    }

    public bool CanCast(int index)
    {
        if (index < 0 || index >= availableConditions.Count) return false;

        foreach (var targetTile in targetTiles)
        {
            if (!availableConditions[index].DoesTileMatchConditionFullParameters(caster?.Tile, targetTile))
                return false;
        }

        return true;
    }
    
    public void AddEffect(string id)
    {
        var index = availableEffects.FindIndex(effect => effect.Id == id);
        AddEffect(index);
    }

    public void AddEffect(int index)
    {
        if (index < 0 || index >= availableEffects.Count) return;

        SelectedEffects.Add(availableEffects[index]);
    }
    
    public bool TryAddEffect(string id)
    {
        var index = availableConditionalEffects.FindIndex(effect => effect.Id == id);
        return TryAddEffect(index);
    }

    public bool TryAddEffect(int index)
    {
        if (index < 0 || index >= availableConditionalEffects.Count) return false;
        
        var conditionalEffect = availableConditionalEffects[index];
        if (!conditionalEffect.DoesTileMatchConditionFullParameters(caster, targetTiles)) return false;
        
        SelectedEffects.AddRange(conditionalEffect.EffectsOnTarget);
        return true;
    }
}

[MoonSharpUserData]
public class AbilityParameterInterpreterText
{
    private NewUnit caster;
    private List<ConditionalEffect<EffectSO>> availableConditionalEffects;
    private List<CustomizableAbilityCondition> availableConditions;
    private List<EffectsOnTarget<EffectSO>> availableEffects;
    private int selectionCount;
    public string ScriptCode { get; private set; }
    public List<string> ReplacedTexts { get; private set; }

    public AbilityParameterInterpreterText(NewUnit caster,
        List<ConditionalEffect<EffectSO>> availableConditionalEffects,
        List<CustomizableAbilityCondition> availableConditions,
        List<EffectsOnTarget<EffectSO>> availableEffects,
        int selectionCount,
        string scriptCode)
    {
        this.caster = caster;
        this.availableConditionalEffects = availableConditionalEffects;
        this.availableConditions = availableConditions;
        this.availableEffects = availableEffects;
        this.selectionCount = selectionCount;
        ScriptCode = scriptCode;
        ReplacedTexts = new List<string>();
    }
    
    public void Log(string message)
    {
        Debug.Log($"Lua: {message}");
    }
    
    public bool CanCast(string id)
    {
        var index = availableConditions.FindIndex(condition => condition.Id == id);
        return CanCast(index);
    }

    public bool CanCast(int index)
    {
        if (index < 0 || index >= availableConditions.Count) return false;
        
        var condition = availableConditions[index];
        
        ReplacedTexts.Add(condition.ConditionText(caster?.Tile, selectionCount));

        return true;
    }
    
    public void AddEffect(string id)
    {
        var index = availableEffects.FindIndex(effect => effect.Id == id);
        AddEffect(index);
    }

    public void AddEffect(int index)
    {
        if (index < 0 || index >= availableEffects.Count) return;

        var effect = availableEffects[index];
        
        ReplacedTexts.Add(effect.Text(caster?.Tile));
    }
    
    public bool TryAddEffect(string id)
    {
        var index = availableConditionalEffects.FindIndex(effect => effect.Id == id);
        return TryAddEffect(index);
    }

    public bool TryAddEffect(int index)
    {
        if (index < 0 || index >= availableConditionalEffects.Count) return false;
        var conditionalEffect = availableConditionalEffects[index];
        
        var conditionText = conditionalEffect.Text(caster?.Tile, selectionCount);
        
        var text = $"If {conditionText.conditionText} then {conditionText.effectText}"; // TODO - do it better
        
        ReplacedTexts.Add(text);

        return true;
    }
}