using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputSystem inputActions;
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;

    private void Awake()
    {
        inputActions ??= new InputSystem();
    }

    private void OnEnable()
    {
        inputActions.Player.Move.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Move.Disable();
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
    }
}
