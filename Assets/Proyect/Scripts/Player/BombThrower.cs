using UnityEngine;

public class BombThrower : MonoBehaviour
{
    [SerializeField] private InputSystem inputActions;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float throwDistance = 10f;
    [SerializeField] private float launchAngle = 45f;

    private void Awake()
    {
        if (inputActions == null)
            inputActions = new InputSystem();

        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (inputActions.Player.Attack.WasPressedThisFrame())
            ThrowBomb();
    }

    private void ThrowBomb()
    {
        if (bombPrefab == null)
            return;

        GameObject bomb = Instantiate(bombPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = CalculateLaunchVelocity();
    }

    private Vector3 CalculateLaunchVelocity()
    {
        float clampedAngle = Mathf.Clamp(launchAngle, 5f, 85f);
        float angleRad = clampedAngle * Mathf.Deg2Rad;
        float gravity = Physics.gravity.magnitude;
        float speed = Mathf.Sqrt((throwDistance * gravity) / Mathf.Sin(2f * angleRad));

        Vector3 horizontalDir = spawnPoint.forward;
        horizontalDir.y = 0f;
        horizontalDir.Normalize();

        return horizontalDir * (Mathf.Cos(angleRad) * speed) + Vector3.up * (Mathf.Sin(angleRad) * speed);
    }
}
