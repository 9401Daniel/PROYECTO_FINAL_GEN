using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInteract : MonoBehaviour
{
    private InputSystem inputActions;

    private InputAction inputInteract;

    private void Awake()
    {
        inputActions ??= new InputSystem();
        inputInteract = inputActions.Player.Interact;
    }

    public void SubscribeToInteract(System.Action<InputAction.CallbackContext> callback)
    {
        inputInteract.performed += callback;
    }

    public void UnsubscribeFromInteract(System.Action<InputAction.CallbackContext> callback)
    {
        inputInteract.performed -= callback;
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.Disable();
    }
}
