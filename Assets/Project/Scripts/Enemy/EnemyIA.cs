using UnityEngine;
using UnityEngine.AI;

// Requiere un NavMeshAgent en el mismo GameObject para poder moverse sobre el NavMesh
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    // ---- Máquina de estados ----
    // Patrol: recorre waypoints en bucle
    // Chase: persigue al jugador porque lo tiene detectado
    // Search: perdió al jugador de vista, va al último punto donde lo vio y espera un rato antes de volver a patrullar
    public enum State { Patrol, Chase, Search }
    public State currentState = State.Patrol; // Estado inicial del enemigo

    [Header("Patrulla")]
    public Transform[] waypoints;   // Puntos que el enemigo recorre en orden mientras patrulla
    public float patrolSpeed = 2f;  // Velocidad de movimiento durante la patrulla
    private int currentWaypointIndex = 0; // Índice del próximo waypoint a visitar

    [Header("Detección (cono de visión)")]
    public float viewRadius = 10f;               // Distancia máxima a la que el enemigo puede detectar al jugador
    [Range(0, 360)] public float viewAngle = 90f; // Apertura del cono de visión, centrado en la dirección hacia donde mira el enemigo
    public LayerMask targetMask;    // Layer que identifica al jugador (para el chequeo de detección)
    public LayerMask obstacleMask;  // Layer de los muros/obstáculos, usada para bloquear la línea de visión

    [Header("Persecución")]
    public float chaseSpeed = 4f; // Velocidad de movimiento al perseguir
    public Transform player;      // Referencia a la posición del jugador

    [Header("Búsqueda (cuando lo pierde de vista)")]
    public float searchTime = 3f;        // Tiempo que el enemigo espera en el último punto conocido antes de rendirse
    private float searchTimer;           // Cuenta regresiva activa mientras está en estado Search
    private Vector3 lastKnownPosition;   // Última posición registrada del jugador antes de perderlo

    private NavMeshAgent agent; // Referencia al componente que maneja el pathfinding y el movimiento

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0) GoToNextWaypoint(); // Arranca la ruta de patrulla si hay waypoints asignados
    }

    void Update()
    {
        // Ejecuta el comportamiento correspondiente al estado actual
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Search: Search(); break;
        }

        // La detección se evalúa en todo momento, sin importar el estado,
        // porque es lo que decide si hay que cambiar de estado
        CheckFieldOfView();
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return; // Sin waypoints no hay ruta que seguir
        agent.speed = patrolSpeed;

        // Si el agente ya no tiene camino pendiente y está cerca del destino, pasa al siguiente waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position); // Envía al agente hacia el waypoint actual
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Avanza el índice en bucle (vuelve a 0 al llegar al final)
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position); // Actualiza el destino cada frame para seguir al jugador en tiempo real
    }

    void Search()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(lastKnownPosition); // Va al último lugar donde vio al jugador
        searchTimer -= Time.deltaTime; // Descuenta el tiempo de búsqueda

        // Cuando llega al punto de búsqueda y el tiempo se agotó, vuelve a patrullar
        if (!agent.pathPending && agent.remainingDistance < 0.5f && searchTimer <= 0f)
        {
            currentState = State.Patrol;
            if (waypoints.Length > 0) GoToNextWaypoint();
        }
    }

    void CheckFieldOfView()
    {
        if (player == null) return; // Sin referencia al jugador no se puede detectar nada

        Vector3 dirToPlayer = (player.position - transform.position).normalized; // Dirección desde el enemigo hacia el jugador
        float distToPlayer = Vector3.Distance(transform.position, player.position); // Distancia entre ambos

        bool inRadius = distToPlayer < viewRadius; // Chequeo de distancia: ¿está dentro del radio de visión?
        bool inAngle = Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f; // Chequeo de ángulo: ¿está dentro del cono de visión?

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

        // Si estaba persiguiendo pero ya no se cumplen las condiciones de detección arriba, pasa a buscar
        if (currentState == State.Chase)
        {
            currentState = State.Search;
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
}