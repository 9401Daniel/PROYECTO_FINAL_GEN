using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Attempts")]
    [SerializeField] private int maxAttempts = 5;
    [SerializeField] private int currentAttempts = 5;

    [Header("Player")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private BombThrower bombThrower;

    private static readonly int CaughtHash = Animator.StringToHash("Caught");

    [Header("Fade")]
    [SerializeField] private float holdDuration = 2f;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Header("Other")]
    [SerializeField] private MissionDetails missionDetails;
    [SerializeField] private TrashMaganer trashManager;
    private bool isBeingCaught;
    public int CurrentAttempts => currentAttempts;

    private event Action onAttemptChanged;

    public event Action OnAttemptChanged
    {
        add { onAttemptChanged += value; }
        remove { onAttemptChanged -= value; }
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        currentAttempts = maxAttempts;
    }

    private void Start()
    {
        if (respawnPoint == null)
            Debug.LogError("Respawn point is not set.");
    }

    public void SetMoving(bool value)
    {
        if (playerMovement != null)
            playerMovement.IsMoving = value;
    }

    /// <summary>
    /// Called by the enemy when it catches the player. Reduces attempts and runs
    /// the caught sequence once. Returns false if already being caught.
    /// </summary>
    public bool LoseAttempt()
    {
        if (isBeingCaught)
            return false;

        if (currentAttempts <= 0)
        {
            // No attempts left: Game Over HERE
            return false;
        }

        currentAttempts--;
        StartCoroutine(CaughtSequence());
        return true;
    }

    private IEnumerator CaughtSequence()
    {
        isBeingCaught = true;
        SetMoving(false);

        // Play Caught animation.
        animator.SetTrigger(CaughtHash);

        // Fade out.
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(holdDuration);
        // Teleport back to start / respawn point.
        transform.parent.position = respawnPoint.position;
        transform.parent.rotation = respawnPoint.rotation;
        missionDetails.ResetCount();
        trashManager.ResetTrash();
        bombThrower.RestoreCharges();
        onAttemptChanged?.Invoke();
        yield return new WaitForSeconds(0.5f);

        // Fade in.
        StartCoroutine(Fade.Instance.FadeIn());

        SetMoving(true);
        isBeingCaught = false;
        print("Player has been caught. attempts: " + currentAttempts);
    }
}
