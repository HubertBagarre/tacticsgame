using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

public class NewAbilityManager : MonoBehaviour
{
    [field: SerializeField] public int CurrentAbilityPoints { get; private set; } = 3;
    [field: SerializeField] public int MaxAbilityPoints { get; private set; } = 6;
    
    private NewUnit caster;
    private UnitAbilityInstance currentCastingAbilityInstance;
    
    void Start()
    {
        
    }

    public void AddCallbacks()
    {
        
    }
    
    public void RemoveCallbacks()
    {
        
    }
}
