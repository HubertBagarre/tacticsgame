using System.Collections;
using System.Collections.Generic;
using Battle;
using TMPro;
using UnityEngine;

public class TileRenderer : MonoBehaviour
{
    public NewTile Tile { get; private set; }
    
    [Header("Anchors")]
    [SerializeField] private Transform modelParent;
    
    [Header("Path Rendering")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject lineRendererGo;
    [SerializeField] private float lineRendererHeight = 0.07f;
    
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

    public void SetTile(NewTile tile)
    {
        Tile = tile;
    }
}
