using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Data Container for Tile Info
    /// (tile position, adjacent Tiles, current Unit)
    /// </summary>
    public class Tile : MonoBehaviour
    {
        //generation
        [SerializeField] private Vector2Int position;
        public Vector2Int Position => position;

        [SerializeField] private Unit currentUnit;

        //pathing
        [Header("Path Rendering")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject lineRendererGo;
        [SerializeField] private float lineRendererHeight = 0.07f;
        [field:Header("Pathing")]
        [field: SerializeField] public bool IsWalkable { get; private set; }
        private int CostToMove => 1; // If we need weights its here lmao (not used btw)
        [SerializeField] private Tile[] neighbors; //0 is top (x,y+1), then clockwise, adjacent before diag
        [field: SerializeField] public int PathRing { get; private set; }

        [Header("Border")]
        [SerializeField] private List<GameObject> bordersGo;
        
        //viusal
        [Header("Visual")]
        [SerializeField] private Renderer modelRenderer;
        [SerializeField] private Material defaultMat;
        [SerializeField] private Material selectableMat;
        [SerializeField] private Material selectedMat;
        [SerializeField] private Material affectedMat;
        [SerializeField] private Material unselectableMat;

        [field: Header("Debug")]
        [field: SerializeField]
        public TextMeshProUGUI DebugText { get; private set; }

        public enum Appearance
        {
            Default,
            Selectable,
            Selected,
            Affected,
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
        
        public Tile GetNeighbor(int direction)
        {
            return direction is < 0 or >= 8 ? null : neighbors[direction];
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

        public Unit Unit => currentUnit;

        public List<Tile> GetAdjacentTiles(Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            return new List<Tile> {neighbors[0], neighbors[1], neighbors[2], neighbors[3]}.Where(tile => tile != null).Where(condition).ToList();
        }

        public List<Tile> GetAdjacentTiles(int range,Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            var start = this;
            
            var frontier = new Queue<Tile>();
            var distanceDict= new Dictionary<Tile,int>();

            frontier.Enqueue(start);
            distanceDict.Add(start,0);
            
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                var distance = distanceDict[current] + 1;
                
                if (distance > range) return distanceDict.Keys.Where(key => key != start).ToList();
                
                foreach (var next in current.GetAdjacentTiles(condition))
                {
                    if (!distanceDict.ContainsKey(next))
                    {
                        frontier.Enqueue(next);
                        distanceDict.Add(next,distance);
                    }
                }
            }
            
            return distanceDict.Keys.Where(key => key != start).ToList();
        }

        public List<Tile> GetSurroundingTiles(Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            return neighbors.Where(tile => tile != null).Where(condition).ToList();
        }

        public List<Tile> GetSurroundingTiles(int range,Func<Tile,bool> condition = null)
        {
            condition ??= _ => true;
            
            var start = this;
            
            var frontier = new Queue<Tile>();
            var distanceDict= new Dictionary<Tile,int>();

            frontier.Enqueue(start);
            distanceDict.Add(start,0);
            
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                var distance = distanceDict[current] + 1;
                
                if (distance > range) return distanceDict.Keys.Where(key => key != start).ToList();
                
                foreach (var next in current.GetSurroundingTiles(condition))
                {
                    if (!distanceDict.ContainsKey(next))
                    {
                        frontier.Enqueue(next);
                        distanceDict.Add(next,distance);
                    }
                }
            }
            
            return distanceDict.Keys.Where(key => key != start).ToList();
        }
        
        public int GetNeighborIndex(Tile tile)
        {
            if(tile == null) return -1;
            if (!GetSurroundingTiles(1).Contains(tile)) return -1;
            for (var i = 0; i < 8; i++)
            {
                if (neighbors[i] == tile) return i;
            }
            return -1;
        }

        public List<Tile> GetTilesInDirection(int direction)
        {
            var startingTile = this;
            var neighborInDirection = startingTile.GetNeighbor(direction);
            var list = new List<Tile>();
            while (neighborInDirection != null)
            {
                list.Add(neighborInDirection);
                neighborInDirection = neighborInDirection.GetNeighbor(direction);
            }

            return list;
        }

        public void SetAppearance(Appearance appearance)
        {
            var mat = appearance switch
            {
                Appearance.Default => defaultMat,
                Appearance.Selectable => selectableMat,
                Appearance.Selected => selectedMat,
                Appearance.Affected => affectedMat,
                Appearance.Unselectable => unselectableMat,
                _ => defaultMat
            };
            
            modelRenderer.material = mat;
        }

        // A* pathfinding, nvm its BFS xd
        // https://www.redblobgames.com/pathfinding/a-star/introduction.html
        public bool GetPath(Tile destination, out List<Tile> path, bool includeDiag = false)
        {
            var start = this;
            
            var frontier = new Queue<Tile>();
            var cameFromDict = new Dictionary<Tile, Tile>();

            frontier.Enqueue(start);
            cameFromDict.Add(start,null);

            path = new List<Tile>{destination};
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == destination)
                {
                    current = destination;

                    while (cameFromDict[current] != start)
                    {
                        path.Add(current);
                        current = cameFromDict[current];
                    }

                    path.Reverse();
                    
                    return true;
                }
                
                foreach (var next in current.GetAdjacentTiles())
                {
                    if (!cameFromDict.ContainsKey(next))
                    {
                        frontier.Enqueue(next);
                        
                        cameFromDict.Add(next,current);
                    }
                }
            }
            
            return false; 
        }

        public void SetPathRing(int value)
        {
            PathRing = value;
            DebugText.text = $"{PathRing}";
        }

        public void HideBorders()
        {
            foreach (var borderGo in bordersGo)
            {
                borderGo.SetActive(false);
            }
        }
        
        public void ShowBorder(int direction, bool value)
        {
            if(direction < 0 || direction >= bordersGo.Count) return;
            bordersGo[direction].SetActive(value);
        }

        public static void ShowBorder(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                for (int direction = 0; direction < 8; direction++)
                {
                    TryShowBorder(direction);
                }
                continue;

                void TryShowBorder(int direction)
                {
                    var adjacentTile = tile.neighbors[direction];
                    if (adjacentTile == null)
                    {
                        tile.ShowBorder(direction,true);
                        return;
                    }

                    if (!tiles.Contains(adjacentTile))
                    {
                        tile.ShowBorder(direction,true);
                    }
                }
            }
            
            
        }

        public void ShowPath()
        {
            lineRendererGo.SetActive(true);
        }

        public void HidePath()
        {
            lineRendererGo.SetActive(false);
        }

        public void SetPath(List<Tile> path)
        {
            var list = path.ToList();
            list.Insert(0,this);
            lineRenderer.positionCount = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                var pos = list[i].transform.position;
                pos.y = lineRendererHeight;
                
                lineRenderer.SetPosition(i, pos);
            }
        }

        public void ClearPath()
        {
            lineRenderer.positionCount = 0;
        }
        
    }
}