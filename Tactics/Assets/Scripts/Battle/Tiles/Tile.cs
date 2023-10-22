using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Data Container for Tile Info
    /// (tile position, adjacent Tiles, current Unit)
    /// </summary>
    public class Tile : MonoBehaviour, IPassivesContainer<Tile>
    {
        //generation
        [SerializeField] private Vector2Int position;
        public Vector2Int Position => position;

        [SerializeField] private Unit currentUnit;
        public List<IEnumerator> OnUnitEnterEvents = new();
        public List<IEnumerator> OnUnitExitEvents = new();

        // Passives
        [Header("Passives")]
        [SerializeField] private Transform passiveAnchor;
        
        public List<PassiveInstance<Tile>> PassiveInstances { get; } = new();
        private List<PassiveInstance<Tile>> passivesToRemove = new();
        public event Action<PassiveInstance<Tile>> OnPassiveAdded; 
        public event Action<PassiveInstance<Tile>> OnPassiveRemoved;
        
        //pathing
        [Header("Path Rendering")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject lineRendererGo;
        [SerializeField] private float lineRendererHeight = 0.07f;
        [field:Header("Pathing")]
        [field: SerializeField] public bool IsWalkable { get; private set; }
        private int CostToMove => 1; // If we need weights its here lmao (not used btw)
        [SerializeField] private Tile[] neighbors; //0 is top (x,y+1), then clockwise, adjacent before diag
        public enum Direction
        {
            Top=0,Right=1,Down=2,Left=3,TopRight=4,DownRight=5,DownLeft=6,TopLeft=7
        }
        
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
        
        [SerializeField] private Vector3 modelPosition = new (0,0.05f,0);
        public Vector3 ModelPosition => transform.position + modelPosition;

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

        public IEnumerator RemoveUnit()
        {
            currentUnit = null;
            var events = OnUnitExitEvents.ToList();
            foreach (var unitEnterEvent in events)
            {
                yield return StartCoroutine(unitEnterEvent);
            }
        }

        public IEnumerator SetUnit(Unit unit)
        {
            currentUnit = unit;
            var events = OnUnitEnterEvents.ToList();
            foreach (var unitEnterEvent in events)
            {
                yield return StartCoroutine(unitEnterEvent);
            }
        }
        
        public void AddOnUnitEnterEvent(IEnumerator unitEnterEvent)
        {
            OnUnitEnterEvents.Add(unitEnterEvent);
        }
        
        public void RemoveOnUnitEnterEvent(IEnumerator unitEnterEvent)
        {
            if(OnUnitEnterEvents.Contains(unitEnterEvent)) OnUnitEnterEvents.Remove(unitEnterEvent);
        }
        
        public void AddOnUnitExitEvent(IEnumerator unitExitEvent)
        {
            OnUnitExitEvents.Add(unitExitEvent);
        }
        
        public void RemoveOnUnitExitEvent(IEnumerator unitExitEvent)
        {
            if(OnUnitExitEvents.Contains(unitExitEvent)) OnUnitExitEvents.Remove(unitExitEvent);
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
        public bool GetPath(Tile destination, out List<Tile> path, bool includeDiag = false,Func<Tile,bool> condition = null)
        {
            //Debug.Log($"Getting path from {name} to {destination.name}");
            
            condition ??= _ => true;
            
            var start = this;
            
            var frontier = new Queue<Tile>();
            var cameFromDict = new Dictionary<Tile, Tile>();

            frontier.Enqueue(start);
            cameFromDict.Add(start,null);

            path = new List<Tile>(); //maybe add {destination}
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current == destination)
                {
                    current = destination;

                    while (current != start)
                    {
                        //Debug.Log($"Adding {current} to path");
                        path.Add(current);
                        current = cameFromDict[current];
                    }
                    
                    path.Reverse();
                    
                    //Debug.Log($"Has path {path.Count} : {path.Aggregate(string.Empty, (current1, p) => current1 + $"{p.name}, ")}");
                    
                    return true;
                }

                var adjacentTiles = includeDiag ? current.GetSurroundingTiles() : current.GetAdjacentTiles();
                foreach (var next in adjacentTiles.Where(condition))
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

        public void ShowLineRendererPath()
        {
            lineRendererGo.SetActive(true);
        }

        public void HideLineRendererPath()
        {
            lineRendererGo.SetActive(false);
        }

        public void SetLineRendererPath(List<Tile> path)
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

        public void ClearLineRendererPath()
        {
            lineRenderer.positionCount = 0;
        }

        public TPassiveInstance GetPassiveInstance<TPassiveInstance>(PassiveSO<Tile> passiveSo) where TPassiveInstance : PassiveInstance<Tile>
        {
            var instance = PassiveInstances.FirstOrDefault(passiveInstance => passiveInstance.SO == passiveSo);
            
            return instance as TPassiveInstance;
        }

        public IEnumerator AddPassiveEffect(PassiveSO<Tile> passiveSo, int amount = 1)
        {
            //add passive instance to list
            if (!passiveSo.IsStackable) amount = 1;
            
            var normalPassive = GetPassiveInstance<PassiveInstance<Tile>>(passiveSo);

            return AddToPassives(normalPassive);
            
            IEnumerator AddToPassives(PassiveInstance<Tile> instance)
            {
                //if current instance == null, no passive yet, creating new and adding to list
                //if passive isn't stackable, creating new and adding to list
                if (instance == null || !passiveSo.IsStackable)
                {
                    instance = passiveSo.CreateInstance<PassiveInstance<Tile>>(amount);
                    if (instance.SO.Model != null)
                    {
                        var model = Instantiate(instance.SO.Model, passiveAnchor);
                        OnPassiveRemoved += RemovePassiveModel;
                    
                        PassiveInstances.Add(instance);
                        
                        Debug.Log($"Added {instance.SO.Name} to {this}");
                    
                        void RemovePassiveModel(PassiveInstance<Tile> passiveInstance)
                        {
                            if(passiveInstance != instance) return;
                        
                            OnPassiveRemoved -= RemovePassiveModel;
                            Destroy(model);
                        }
                    }
                }
            
                OnPassiveAdded?.Invoke(instance);
                
                return instance.AddPassive(this);
            }
        }

        public IEnumerator RemovePassiveEffect(PassiveSO<Tile> passiveSo)
        {
            var currentInstance = GetPassiveInstance<PassiveInstance<Tile>>(passiveSo);
            return currentInstance == null ? null : RemovePassiveEffect(currentInstance);
        }

        public IEnumerator RemovePassiveEffect(PassiveInstance<Tile> passiveInstance)
        {
            if (!PassiveInstances.Contains(passiveInstance)) return null;
            PassiveInstances.Remove(passiveInstance);
            
            OnPassiveRemoved?.Invoke(passiveInstance);
            
            return passiveInstance.RemovePassive(this);
        }
        
        private IEnumerator RemoveAllPassivesToRemove()
        {
            foreach (var passiveToRemove in passivesToRemove)
            {
                yield return StartCoroutine(RemovePassiveEffect(passiveToRemove)); 
            }
            passivesToRemove.Clear();
        }

        public int GetPassiveEffectCount(Func<PassiveInstance<Tile>, bool> condition, out PassiveInstance<Tile> firstPassiveInstance)
        {
            condition ??= _ => true;
            
            firstPassiveInstance = PassiveInstances.Where(condition).FirstOrDefault();
            
            return PassiveInstances.Count(condition);
        }
    }
}