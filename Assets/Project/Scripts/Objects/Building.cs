using UnityEngine;
using UnityEngine.InputSystem;

public class Building : Interactable
{
    protected override void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact building.");
    }
}
