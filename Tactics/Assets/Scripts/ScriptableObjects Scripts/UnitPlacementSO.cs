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
            public Unit prefab;
            public UnitRenderer rendererPrefab;
            public UnitSO so;
            public int team;
            public Vector2Int position;
            public NewTile.Direction orientation;
            public bool asPlayer;
            public bool no;
        }
    }
}


