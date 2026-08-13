using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;

    private PlayerInteract playerInRange;
    private Camera mainCamera;

    private void Awake()
    {
        promptUI?.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange == null)
            return;

        if (promptUI != null && promptUI.activeSelf)
        {
            mainCamera ??= Camera.main;
            if (mainCamera != null)
                promptUI.transform.rotation = Quaternion.LookRotation(promptUI.transform.position - mainCamera.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (playerInRange != null)
            return;
        if (other.CompareTag("Player"))
        {
            PlayerInteract player = other.GetComponent<PlayerInteract>();
            if (player != null)
            {
                print("OnTriggerEnter base.");
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
            print("OnTriggerExit base.");
            HidePrompt();
            player.UnsubscribeFromInteract(Interact);
            playerInRange = null;
        }
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
