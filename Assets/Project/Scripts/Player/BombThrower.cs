using System.Collections;
using UnityEngine;

public class BombThrower : MonoBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnOffset = 0.5f;
    [Header("Bomb Physics")]
    [SerializeField, Min(1f)] private float throwDistance = 2f;
    [SerializeField, Range(5f, 85f)] private float launchAngle = 45f; // este rango evita tener ángulos muy pequeños o muy grandes donde no se veria como un lanzamiento
    [Header("Thrower Configuration")]
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private float cooldownPerCharge = 60f;

    private InputSystem inputActions;
    private Vector3 lastDirection;
    private int currentCharges;
    private float cooldownTimer;
    private Coroutine cooldownLogger;

    private void Awake()
    {
        inputActions ??= new InputSystem();

        if (spawnPoint == null)
        {
            GameObject spawn = new GameObject("BombSpawnPoint");
            spawn.transform.SetParent(transform);
            spawnPoint = spawn.transform;
        }
    }

    private void OnEnable()
    {
        currentCharges = maxCharges;
        cooldownTimer = 0f;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
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
            ThrowBomb();
    }

    public void RestoreCharges(int amount)
    {
        currentCharges = Mathf.Min(currentCharges + amount, maxCharges);
    }

    private void ThrowBomb()
    {
        if (bombPrefab == null || currentCharges <= 0 || cooldownTimer > 0f)
            return;

        currentCharges--;
        cooldownTimer = cooldownPerCharge;

        cooldownLogger ??= StartCoroutine(LogCooldown());

        Vector3 facing = lastDirection != Vector3.zero ? lastDirection : Vector3.right;// Si no hay dirección, apunta hacia la derecha

        spawnPoint.position = transform.position + facing * spawnOffset;
        spawnPoint.rotation = Quaternion.LookRotation(facing);

        GameObject bomb = Instantiate(bombPrefab, spawnPoint.position, spawnPoint.rotation);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = CalculateLaunchVelocity();
    }

    private IEnumerator LogCooldown()
    {
        while (cooldownTimer > 0f)
        {
            Debug.Log($"Cooldown: {cooldownTimer:F1}s");
            yield return new WaitForSeconds(1f);
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
}
