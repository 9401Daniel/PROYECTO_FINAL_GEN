using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;

    private PlayerInteract playerInRange;
    protected bool active = true;

    private void Awake()
    {
        promptUI?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerInRange != null || !active)
            return;
        if (other.CompareTag("Player"))
        {
            PlayerInteract player = other.GetComponent<PlayerInteract>();
            if (player != null)
            {
                ShowPrompt();
                playerInRange = player;
                player.SubscribeToInteract(Interact);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerInRange == null)
            return;
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null && player == playerInRange)
        {
            FinishInteraction();
        }
    }

    protected void FinishInteraction()
    {
        HidePrompt();
        playerInRange.UnsubscribeFromInteract(Interact);
        playerInRange = null;
    }

    protected virtual void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact base." + context);
    }

    protected void ShowPrompt()
    {
        promptUI?.SetActive(true);
    }

    protected void HidePrompt()
    {
        promptUI?.SetActive(false);
    }
}
