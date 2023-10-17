using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Battle Scriptables/Unit Placement")]
    public class UnitPlacementSO : ScriptableObject
    {
        [field:Header("Unit Parameters")]
        [field:SerializeField] public List<PlacedUnit> PlacedUnits{ get; private set; }

        [Serializable]
        public class PlacedUnit
        {
            public UnitStatsSO so;
            public Vector2Int position;
            public int team;
            public Unit prefab;
        }
    }
}


