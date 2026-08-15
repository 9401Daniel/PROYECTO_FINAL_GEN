using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputSystem inputActions;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private int currentDirection = 0; // 0=Down 1=Side 2=Forward
    private bool isMoving = true;

    public bool IsMoving { set { isMoving = value; } }

    private void Awake()
    {
        inputActions ??= new InputSystem();
        if (animator == null)
        {
            Debug.LogError("Animator Component not found");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer Component not found");
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Attack.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Attack.Disable();
    }

    void Update()
    {
        if (!isMoving)
        {
            animator.SetInteger("MovementState", 0);
            moveInput = Vector2.zero;
            return;
        }
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Move();
    }
    private void Move()
    {
        UpdateDirection();
        UpdateAnimation();
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
    }

    private void UpdateDirection()
    {
        if (moveInput == Vector2.zero)
            return;
        // Side Movement
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            currentDirection = 1;
            //Side Orientation
            if (moveInput.x > 0)
                spriteRenderer.flipX = false;
            else
                spriteRenderer.flipX = true;
        }
        // Forward Movement
        else if (moveInput.y > 0)
        {
            currentDirection = 2;
            spriteRenderer.flipX = false;
        }
        // Down Movement
        else if (moveInput.y < 0)
        {
            currentDirection = 0;
            spriteRenderer.flipX = false;
        }

        animator.SetFloat("Direction", currentDirection);
    }

    private void UpdateAnimation()
    {
        //WalkingState = 1
        if (moveInput != Vector2.zero)
        {
            animator.SetInteger("MovementState", 1);
        }
        //IdleState = 0
        else
        {
            animator.SetInteger("MovementState", 0);
        }
    }
}
