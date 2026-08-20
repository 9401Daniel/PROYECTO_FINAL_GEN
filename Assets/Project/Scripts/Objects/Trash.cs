using UnityEngine;
using UnityEngine.InputSystem;

public class Trash : Interactable
{
    protected override void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact Trash.");
        active = false;
    }
}
