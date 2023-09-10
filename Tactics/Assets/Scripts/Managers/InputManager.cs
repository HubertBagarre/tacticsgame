using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Controls inputs;

    public static event Action LeftClickEvent;
    public static event Action RightClickEvent;
    
    private void Awake()
    {
        SetupInputMap();
    }

    private void SetupInputMap()
    {
        inputs = new Controls();
        inputs.Enable();
    }
    
    public void InvokeLeftClickEvent(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        LeftClickEvent?.Invoke();
    }

    public void InvokeRightClickEvent(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        RightClickEvent?.Invoke();
    }

    public static void CastCamRay(out RaycastHit hit, LayerMask layerMask)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            hit = new RaycastHit();
            return;
        }

        var mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 100f);

        Physics.Raycast(mouseRay, out hit, Mathf.Infinity, layerMask);
    }
}