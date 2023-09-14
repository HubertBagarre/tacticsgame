using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Data Container for Tile Info
    /// (tile position, adjacent Tiles, current Unit)
    /// </summary>
    public class Tile : MonoBehaviour
    {
        //identification
        [SerializeField] private Vector2Int position;
        public Vector2Int Position => position;

        [SerializeField] private Unit currentUnit;

        //pathing
        [field: SerializeField] public bool IsWalkable { get; private set; }
        [SerializeField] private Tile[] neighbors; //0 is top (x,y+1), then clockwise, adjacent before diag
        [field: SerializeField] public int PathRing { get; private set; }

        //viusal
        [Header("Visual")] [SerializeField] private Renderer modelRenderer;

        [SerializeField] private Material defaultMat;
        [SerializeField] private Material selectableMat;
        [SerializeField] private Material selectedMat;
        [SerializeField] private Material unselectableMat;

        [field: Header("Debug")]
        [field: SerializeField]
        public TextMeshProUGUI DebugText { get; private set; }

        public enum Appearance
        {
            Default,
            Selectable,
            Selected,
            Unselectable,
        }

        public void InitPosition(int x, int y)
        {
            position = new Vector2Int(x, y);
        }

        public void InitNeighbors(Tile[] tiles)
        {
            neighbors = tiles;
        }

        public void SetWalkable(bool value)
        {
            IsWalkable = value;
        }

        public void RemoveUnit()
        {
            currentUnit = null;
        }

        public void SetUnit(Unit unit)
        {
            currentUnit = unit;
        }

        public bool HasUnit()
        {
            return currentUnit != null;
        }

        public Unit GetUnit()
        {
            return currentUnit;
        }

        public List<Tile> GetAdjacentTiles(Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            return new List<Tile> {neighbors[0], neighbors[1], neighbors[2], neighbors[3]}.Where(tile => tile != null).Where(condition).ToList();
        }

        public List<Tile> GetSurroundingTiles(Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            return neighbors.Where(tile => tile != null).Where(condition).ToList();
        }

        public List<Tile> GetSurroundingTiles(int range,Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;

            var tiles = new List<Tile>();

            if (range <= 0) return tiles;
            if (range == 1)
            {
                tiles.AddRange(GetSurroundingTiles().Where(condition));
                return tiles;
            }
            
            // TODO - do kinda of the same as below, but return list instead of bool (return already visited xd)
            
            return tiles;
        }

        /// <summary>
        /// Tries to path from this to targetTile
        /// </summary>
        /// <param name="targetTile"></param>
        /// <param name="range"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool IsInSurroundingTileDistance(Tile targetTile,int range,Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            var found = false;
            var iteration = 1;
            var alreadyvisited = new List<Tile>();
            var surroundingTiles = GetSurroundingTiles(condition);

            return this == targetTile || surroundingTiles.Contains(targetTile) || SearchInTiles(surroundingTiles);

            bool SearchInTiles(List<Tile> previouslySearchedTiles)
            {
                iteration++;
                if (iteration > range) return false;
                alreadyvisited.AddRange(surroundingTiles);
                surroundingTiles = new List<Tile>();
                

                foreach (var previouslySearchedTile in previouslySearchedTiles)
                {
                    surroundingTiles.AddRange(previouslySearchedTile.GetSurroundingTiles(condition)
                        .Where(tile => !alreadyvisited.Contains(tile))
                        .Where(tile => !surroundingTiles.Contains(tile)));
                }
                
                return surroundingTiles.Contains(targetTile) || SearchInTiles(surroundingTiles);
            }
        }
        
        /// <summary>
        /// Tries to path from this to targetTile
        /// </summary>
        /// <param name="targetTile"></param>
        /// <param name="range"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool IsInAdjacentTileDistance(Tile targetTile,int range,Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            var found = false;
            var iteration = 1;
            var alreadyvisited = new List<Tile>();
            var surroundingTiles = GetAdjacentTiles(condition);
            
            return this == targetTile || surroundingTiles.Contains(targetTile) || SearchInTiles(surroundingTiles);

            bool SearchInTiles(List<Tile> previouslySearchedTiles)
            {
                iteration++;
                if (iteration > range) return false;
                alreadyvisited.AddRange(surroundingTiles);
                surroundingTiles = new List<Tile>();

                foreach (var previouslySearchedTile in previouslySearchedTiles)
                {
                    surroundingTiles.AddRange(previouslySearchedTile.GetAdjacentTiles(condition)
                        .Where(tile => !alreadyvisited.Contains(tile))
                        .Where(tile => !surroundingTiles.Contains(tile)));
                }
                
                return surroundingTiles.Contains(targetTile) || SearchInTiles(surroundingTiles);
            }
        }

        public void SetAppearance(Appearance appearance)
        {
            Material mat = appearance switch
            {
                Appearance.Default => defaultMat,
                Appearance.Selectable => selectableMat,
                Appearance.Selected => selectedMat,
                Appearance.Unselectable => unselectableMat,
                _ => defaultMat
            };

            modelRenderer.material = mat;
        }

        public void SetPathRing(int value)
        {
            PathRing = value;
            DebugText.text = $"{PathRing}";
        }
    }
}