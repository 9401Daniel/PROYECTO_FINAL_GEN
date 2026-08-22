using UnityEngine;
using UnityEngine.AI;

// Requiere un NavMeshAgent en el mismo GameObject para poder moverse sobre el NavMesh
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search, Alert, MovingToNoise, Catch }
    public State currentState = State.Patrol;

    [Header("Patrulla")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    private int currentWaypointIndex = 0;

    [Header("Detección (cono de visión)")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Persecución")]
    public float chaseSpeed = 4f;
    public Transform player;

    [Header("Búsqueda (cuando lo pierde de vista)")]
    public float searchTime = 3f;
    private float searchTimer;
    private Vector3 lastKnownPosition;

    [Header("Alerta (bombas de ruido)")]
    public float alertTime = 1.5f;
    private float alertTimer;
    private Vector3 noisePosition;

    [Header("Animación")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRendered;

    [Header("Captura")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float catchDuration = 1.5f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
    private static readonly int AlertTriggerHash = Animator.StringToHash("AlertTrigger");
    private static readonly int CatchPlayerHash = Animator.StringToHash("CatchPlayer");

    private PlayerStats capturedStats;
    private bool catchTriggered;
    private float catchTimer;
    private NavMeshAgent agent;

    // Reinicia por completo el estado del enemigo para poder reutilizarlo
    // (por ejemplo, al reiniciar el nivel o respawnear a los enemigos).
    // Vuelve al estado de patrulla, limpia alerta/búsqueda/captura y
    // reactiva el NavMeshAgent.
    public void ResetEnemy()
    {
        // Vuelve al estado inicial de patrulla
        currentState = State.Patrol;

        // Limpia búsqueda y alerta
        searchTimer = 0f;
        alertTimer = 0f;
        noisePosition = Vector3.zero;

        // Limpia captura
        capturedStats = null;
        catchTriggered = false;
        catchTimer = 0f;

        // Reinicia el índice de waypoints para empezar de nuevo la ruta
        currentWaypointIndex = 0;

        // Reactiva el NavMeshAgent y limpia cualquier ruta pendiente
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Lo envía de nuevo al primer waypoint para retomar la patrulla
        if (waypoints.Length > 0 && agent != null)
            agent.SetDestination(waypoints[0].position);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0) GoToNextWaypoint();
    }

    void Update()
    {
        // Ejecuta el comportamiento correspondiente al estado actual
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Alert: Alert(); break;
            case State.MovingToNoise: MovingToNoise(); break;
            case State.Catch: Catch(); break;
        }

        // La detección se evalúa en todo momento, sin importar el estado,
        // porque es lo que decide si hay que cambiar de estado
        CheckFieldOfView();
        UpdateAnimator();
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    // Estado mientras el enemigo espera quieto haciendo la animación de alerta.
    // No se mueve todavía: solo reacciona al ruido quedándose un momento.
    void Alert()
    {
        agent.isStopped = true;

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
        {
            agent.isStopped = false;
            currentState = State.MovingToNoise;
            agent.SetDestination(noisePosition); // Ya salió la animación de alerta: ahora corre hacia el ruido
        }
    }

    // Corre (chaseSpeed) hacia el punto del ruido hasta llegar.
    void MovingToNoise()
    {
        agent.speed = chaseSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.01f)
        {
            currentState = State.Patrol;
            searchTimer = searchTime;
        }
    }

    // Llamado desde afuera (por ejemplo, desde el script de la bomba de ruido) cuando el enemigo
    // está dentro del radio de alcance del sonido
    public void HearNoise(Vector3 position)
    {
        // Si ya está persiguiendo o capturando al jugador, el ruido no lo distrae
        if (currentState == State.Chase || currentState == State.Catch) return;

        noisePosition = position;
        currentState = State.Alert;
        alertTimer = alertTime;
        agent.isStopped = true;

        if (anim != null)
            anim.SetTrigger(AlertTriggerHash);
    }


    void CheckFieldOfView()
    {
        if (currentState == State.Catch)
            return;

        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        bool inRadius = distToPlayer < viewRadius;
        bool inAngle = Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f;

        if (inRadius && inAngle)
        {
            // Raycast desde el enemigo hacia el jugador: confirma que no hay un muro tapando la línea de visión
            bool blocked = Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask);

            if (!blocked)
            {
                // Detección confirmada: el enemigo realmente ve al jugador
                lastKnownPosition = player.position; // Guarda la posición por si luego se pierde el contacto visual
                currentState = State.Chase;
                searchTimer = searchTime; // Reinicia el contador de búsqueda para cuando lo pierda de vista
                return;
            }
        }
        if (currentState == State.Chase)
        {
            currentState = State.Patrol;
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

            if (dir.x > 0)
            {
                spriteRendered.flipX = false;
            }
            else
            {
                spriteRendered.flipX = true;
            }
        }

    }

    // Dibuja el radio y el cono de visión en la vista de Scene, solo quando el objeto está seleccionado
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, viewRadius); // Círculo que representa el radio de detección

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2f);   // Límite izquierdo del cono
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2f);   // Límite derecho del cono

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }

    // Convierte un ángulo relativo a la rotación del enemigo en un vector de dirección en el mundo
    Vector3 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y; // Ajusta el ángulo según hacia dónde mira el enemigo
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void Catch()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Avisa al jugador una sola vez mientras dura el estado.
        if (!catchTriggered && capturedStats != null)
        {
            catchTriggered = true;
            capturedStats.SetMoving(false);
            capturedStats.LoseAttempt();
        }

        anim.SetTrigger(CatchPlayerHash);

        // Tras el tiempo de captura, vuelve a patrullar.
        if (catchTimer > 0f)
        {
            catchTimer -= Time.deltaTime;
        }
        else
        {
            catchTimer = catchDuration;
            currentState = State.Patrol;
            capturedStats = null;
            catchTriggered = false;
            if (waypoints.Length > 0) GoToNextWaypoint();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignora si ya está capturando o si no toca al jugador.
        if (currentState == State.Catch)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !collision.collider.CompareTag(playerTag))
            return;

        PlayerStats stats = collision.collider.GetComponentInParent<PlayerStats>();
        if (stats == null)
            return;

        capturedStats = stats;
        catchTriggered = false;
        catchTimer = catchDuration;
        currentState = State.Catch;
    }
}
