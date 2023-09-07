using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit")]
public class UnitStatsSO : ScriptableObject
{
    [field: SerializeField] public int movement;
}
