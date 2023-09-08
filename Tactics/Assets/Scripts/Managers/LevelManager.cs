using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [field: SerializeField] private TileManager tileManager;

    private void Start()
    {
        tileManager.ConnectInputs();
        
        Run();
    }

    public void Run()
    {
        foreach (var tile in tileManager.AllTiles)
        {
            tile.SetMat(Tile.Appearance.Unselected);
        }
    }
}
