using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class BombThrower : MonoBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnOffset = 0.5f;
    [Header("Bomb Physics")]
    [SerializeField, Min(1f)] private float throwDistance = 2f;
    [SerializeField, Range(5f, 85f)] private float launchAngle = 45f; // este rango evita tener ángulos muy pequeños o muy grandes donde no se vería como un lanzamiento.
    [Header("Thrower Configuration")]
    [SerializeField] private Animator animator;
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private float cooldownPerCharge = 60f;

    private InputSystem inputActions;
    private Vector3 lastDirection;
    private int currentCharges;
    private float cooldownTimer;
    private Coroutine cooldownLogger;
    private bool isThrowing = false;
    private Vector3 spawnReference;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        inputActions ??= new InputSystem();

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint is not assigned.");
        }
        else
        {
            spawnReference = spawnPoint.localPosition;
        }

        playerMovement = GetComponent<PlayerMovement>();
    }

    public void RestoreCharges(int amount)
    {
        currentCharges = Mathf.Min(currentCharges + amount, maxCharges);
    }

    private void OnEnable()
    {
        currentCharges = maxCharges;
        cooldownTimer = 0f;
        inputActions.Player.Move.Enable();
        inputActions.Player.Attack.Enable();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.sqrMagnitude > 0.001f) //uso de magnitud cuadrada para evitar calcular la raíz cuadrada para la distancia.
            lastDirection = inputDirection.normalized;

        if (inputActions.Player.Attack.WasPressedThisFrame())
            StartThrow();
    }

    private void StartThrow()
    {
        if (currentCharges <= 0 || cooldownTimer > 0f)
            return;

        currentCharges--;
        cooldownTimer = cooldownPerCharge;
        OnThrowAnimation();
        cooldownLogger ??= StartCoroutine(LogCooldown());
    }

    /// <summary>
    /// Lanza la bomba en la dirección del último movimiento. Se activa desde la animación de lanzamiento como un evento.
    /// </summary>
    private void DropBomb()
    {
        Vector3 facing = lastDirection != Vector3.zero ? lastDirection : Vector3.forward * -1f;// Si no hay dirección, apunta hacia adelante

        switch (facing.z)
        {
            case 0:
                spawnPoint.localPosition = spawnReference + facing * spawnOffset;
                break;
            case > 0:
                spawnPoint.localPosition = spawnReference + new Vector3(spawnOffset, 0f, spawnOffset);
                break;
            case < 0:
                spawnPoint.localPosition = spawnReference + new Vector3(-spawnOffset, 0f, -spawnOffset);
                break;
        }
        spawnPoint.localRotation = Quaternion.LookRotation(facing);

        GameObject bomb = Instantiate(bombPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = CalculateLaunchVelocity();
    }

    private void OnThrowAnimation()
    {
        if (isThrowing)
            return;
        isThrowing = true;
        animator.SetTrigger("Throw");
        playerMovement.IsMoving = false;
    }

    /// <summary>
    /// Permite al jugador moverse nuevamente después de lanzar la bomba. Se activa desde la animación de lanzamiento como un evento.
    /// </summary>
    private void FinishThrow()
    {
        playerMovement.IsMoving = true;
        isThrowing = false;
    }

    private IEnumerator LogCooldown()
    {
        while (cooldownTimer > 0f)
        {
            Debug.Log($"Cooldown: {cooldownTimer:F1}s");
            yield return new WaitForSeconds(1.6f);
        }

        cooldownLogger = null;
    }


    /// <summary>
    /// Calcula la velocidad de lanzamiento de la bomba. Todo parte de la formula alcance = (v² · sin(2θ)) / g. Despejando v = velosidad, obtenemos la velocidad de lanzamiento para un alcance dado.
    /// </summary>
    /// <returns>La velocidad de lanzamiento.</returns>
    private Vector3 CalculateLaunchVelocity()
    {
        float angleRad = launchAngle * Mathf.Deg2Rad;
        float gravity = Physics.gravity.magnitude;
        float speed = Mathf.Sqrt(throwDistance * gravity / Mathf.Sin(2f * angleRad)); // v = √[(distancia · g) / sin(2θ)]

        Vector3 horizontalDir = spawnPoint.forward;
        horizontalDir.y = 0f;
        horizontalDir.Normalize();

        return horizontalDir * (Mathf.Cos(angleRad) * speed) + Vector3.up * Mathf.Sin(angleRad) * speed; //tiro parabolico en el eje horizontal y en el eje vertical en la dirección indicada por spawnPoint
    }

    private void OnDisable()
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Attack.Disable();
    }
}
