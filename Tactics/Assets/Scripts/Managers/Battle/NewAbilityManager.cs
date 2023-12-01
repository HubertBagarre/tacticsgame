using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

public class NewAbilityManager : MonoBehaviour
{
    [SerializeField] private int currentAbilityPoints = 0;
    public int CurrentAbilityPoints
    {
        get => currentAbilityPoints;
        private set
        {
            var previous = currentAbilityPoints;
            currentAbilityPoints = value;
            OnUpdatedAbilityPoints?.Invoke(previous, currentAbilityPoints);
        }
        
    }
    [field: SerializeField] public int MaxAbilityPoints { get; private set; } = 6;
    public static event Action<int, int> OnUpdatedAbilityPoints;

    public void Setup(int startingAbilityPoints)
    {
        currentAbilityPoints = startingAbilityPoints;
    }
    
    public void AddCallbacks()
    {
        
    }
    
    public void RemoveCallbacks()
    {
        
    }
    
    
}
