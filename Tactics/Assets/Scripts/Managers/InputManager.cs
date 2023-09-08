using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private Camera cam;

    [Header("Settings")] [SerializeField] protected LayerMask entityLayers;
    [SerializeField] protected LayerMask worldLayers;

    protected Controls inputs;

    [field:Header("Debug")]
    [field:SerializeField] public Tile SelectedTile { get; private set; }
    [field:SerializeField] public Unit SelectedUnit { get; private set; }

    private void Awake()
    {
        SetupInputMap();
    }

    private void SetupInputMap()
    {
        inputs = new Controls();
        inputs.Enable();
    }
    
    protected void UpdateTargets()
    {
        CastCamRay(out var unitHit, out var tileHit);
        
        if (tileHit.transform != null)
        {
            SelectedTile = tileHit.transform.GetComponent<Tile>();
        }
        else
        {
            SelectedTile = null;
        }
        
        if (unitHit.transform != null)
        {
            SelectedUnit = unitHit.transform.GetComponent<Unit>();
        }
        else
        {
            SelectedUnit = null;
        }
    }

    private void CastCamRay(out RaycastHit entityHit, out RaycastHit worldHit)
    {
        if (cam == null)
        {
            entityHit = new RaycastHit();
            worldHit = new RaycastHit();
            return;
        }

        var mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 100f);

        Physics.Raycast(mouseRay, out entityHit, Mathf.Infinity, entityLayers);
        Physics.Raycast(mouseRay, out worldHit, Mathf.Infinity, worldLayers);
    }

    public void LeftClickEvent(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        UpdateTargets();
        Debug.Log("LeftClick");
    }

    public void RightClickEvent(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        UpdateTargets();
        Debug.Log("RightClick");
    }
}