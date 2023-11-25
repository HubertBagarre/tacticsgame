using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Controls inputs;
    private static Camera cam;

    public static event Action LeftClickEvent;
    public static event Action RightClickEvent;
    
    private void Awake()
    {
        SetupInputMap();
    }

    private void SetupInputMap()
    {
        inputs = new Controls();
        
        inputs.InGame.MouseLeftClick.started += InvokeLeftClickEvent;
        inputs.InGame.MouseRightClick.started += InvokeRightClickEvent;
        
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
    
    public void SetupCamera(Camera c)
    {
        cam = c;
    }

    public static void CastCamRay(out RaycastHit hit, LayerMask layerMask)
    {
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