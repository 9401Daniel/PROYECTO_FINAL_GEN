using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputSystem inputActions;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private int currentDirection = 0; // 0=Down 1=Side 2=Forward
    private bool isThrowing = false;

    private void Awake()
    {
        inputActions ??= new InputSystem();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Attack.Enable();

        //inputActions.Player.Attack.performed += OnThrow;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Attack.Disable();

        //inputActions.Player.Attack.performed -= OnThrow;
    }

    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        //Lock Movement while throwing
        if (isThrowing)
        {
            moveInput = Vector2.zero;
            return;
        }
        UpdateDirection();
        UpdateAnimation();
        Move();
    }

    /*
    private void OnThrow(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isThrowing)
            return;
        isThrowing = true;
        animator.SetTrigger("Throw");
    }

    public void FinishThrow()
    {
        isThrowing = false;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        UpdateDirection();
        UpdateAnimation();
    } */

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

    private void Move()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
    }
}
