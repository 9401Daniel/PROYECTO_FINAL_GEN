using UnityEngine;
using UnityEngine.InputSystem;

public class Trash : Interactable
{
    [SerializeField] private MissionDetails missionDetails;
    protected override void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact Trash.");
        missionDetails.AddCount();
        active = false;
        FinishInteraction();
    }
}
