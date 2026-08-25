using UnityEngine;
using UnityEngine.AI;

// Requiere un NavMeshAgent en el mismo GameObject para poder moverse sobre el NavMesh
// Version recortada de EnemyAI: solo Patrol + Animacion, sin deteccion/persecucion/captura
[RequireComponent(typeof(NavMeshAgent))]
public class NPCWalker : MonoBehaviour
{
    public enum State { Patrol, Idle }
    public State currentState = State.Patrol;

    [Header("Patrulla")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    private int currentWaypointIndex = 0;

    [Header("Espera en cada waypoint")]
    [Tooltip("Si está en 0, el NPC no se detiene: pasa de un waypoint a otro sin pausas, igual que Enemy.")]
    public float waitTime = 0f;
    private float waitTimer;

    [Header("Animación")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRendered;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash = Animator.StringToHash("MoveZ");

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Si no arrastraron el Animator/SpriteRenderer a mano, los busca solo en "Visual"
        if (anim == null || spriteRendered == null)
        {
            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                if (anim == null) anim = visual.GetComponent<Animator>();
                if (spriteRendered == null) spriteRendered = visual.GetComponent<SpriteRenderer>();
            }
        }

        if (waypoints.Length > 0) GoToNextWaypoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Idle: Idle(); break;
        }

        UpdateAnimator();
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (waitTime > 0f)
            {
                currentState = State.Idle;
                waitTimer = waitTime;
                agent.isStopped = true;
            }
            else
            {
                GoToNextWaypoint();
            }
        }
    }

    // Igual que en Enemy.Alert(): se queda quieto un momento antes de seguir la ruta.
    // Solo se usa si waitTime > 0.
    void Idle()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            agent.isStopped = false;
            currentState = State.Patrol;
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    public void ResetNPC()
    {
        currentState = State.Patrol;
        waitTimer = 0f;
        currentWaypointIndex = 0;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            if (agent != null)
                agent.Warp(waypoints[0].position);
        }
    }

    void UpdateAnimator()
    {
        if (anim == null) return;
        float currentSpeed = agent.velocity.magnitude;
        anim.SetFloat(SpeedHash, currentSpeed);

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 dir = agent.velocity.normalized;
            anim.SetFloat(MoveXHash, Mathf.Abs(dir.x));
            anim.SetFloat(MoveZHash, dir.z);

            if (spriteRendered != null)
                spriteRendered.flipX = dir.x <= 0;
        }
    }
}