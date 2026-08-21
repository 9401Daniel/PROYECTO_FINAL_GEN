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
    public enum State { Patrol, Chase, Search, Alert}
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

    [Header("Alerta (bombas de ruido)")]
    public float alertTime = 3f;         // Tiempo que se queda "revisando" el punto del ruido antes de volver a patrullar
    public float lookAroundSpeed = 60f;  // Velocidad de giro (grados/seg) mientras está en alerta, simula que está revisando la zona
    private float alertTimer;            // Cuenta regresiva activa mientras está en estado Alert
    private Vector3 noisePosition;       // Posición donde ocurrió el ruido


   // ======================= AGREGADO: ANIMATOR =======================

    [Header("Animación")]
    [SerializeField] private Animator anim;              // Animator del sprite (normalmente en un hijo "Visual")
    [SerializeField] private Transform spriteTransform;   // Transform de ese mismo hijo, para poder flipearlo y evitar que rote
 
    // Hasheamos los nombres de los parámetros una sola vez: es más rápido que pasar el string cada frame
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
    private static readonly int IsAlertHash = Animator.StringToHash("IsAlert");

    // ====================================================================


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
            case State.Alert: Alert(); break;
        }

        // La detección se evalúa en todo momento, sin importar el estado,
        // porque es lo que decide si hay que cambiar de estado
        CheckFieldOfView();
        UpdateAnimator();
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


    void Alert()
    {
        agent.speed = patrolSpeed;

        // Mientras se acerca al punto del ruido, sigue moviéndose normal
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Ya llegó: se queda girando en el lugar, "revisando" la zona
            agent.isStopped = true;
            transform.Rotate(Vector3.up, lookAroundSpeed * Time.deltaTime);

            alertTimer -= Time.deltaTime;
            if (alertTimer <= 0f)
            {
                agent.isStopped = false;
                currentState = State.Patrol;
                if (waypoints.Length > 0) GoToNextWaypoint();
            }
        }
    }

    // Llamado desde afuera (por ejemplo, desde el script de la bomba de ruido) cuando el enemigo
    // está dentro del radio de alcance del sonido
    public void HearNoise(Vector3 position)
    {
        // Si ya está persiguiendo al jugador, el ruido no lo distrae de la persecución
        if (currentState == State.Chase) return;

        noisePosition = position;
        currentState = State.Alert;
        alertTimer = alertTime;
        agent.isStopped = false;
        agent.SetDestination(noisePosition);
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



    // ======================= AGREGADO: ANIMATOR =======================
    // Traduce lo que está pasando en la máquina de estados a parámetros que el
    // Animator Controller entiende. Esta función NO decide qué animación se ve,
    // solo pasa datos — la decisión final la toma el grafo del Animator.
    void UpdateAnimator()
    {
        if (anim == null) return; // por si todavía no conectaste el Animator, no rompe el resto del script
 
        // agent.velocity es la velocidad REAL de movimiento (mundo), no depende de hacia dónde mira el objeto.
        // Dividir por agent.speed la normaliza entre 0 y 1, útil para el Blend Tree de Walk/Run.
        //float speedNormalized = agent.speed > 0.01f ? agent.velocity.magnitude / agent.speed : 0f;
        //anim.SetFloat(SpeedHash, speedNormalized);
        float currentSpeed = agent.velocity.magnitude;
        anim.SetFloat(SpeedHash, currentSpeed);
        // Solo actualizamos la dirección si realmente se está moviendo, para que no "tiemble"
        // el Blend Tree cuando el enemigo está parado (ej. en Alert ya detenido).
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 dir = agent.velocity.normalized;
            anim.SetFloat(MoveXHash, Mathf.Abs(dir.x));
            anim.SetFloat(MoveZHash, dir.z);
 
            // Flip horizontal: reusamos el mismo clip "Side" para izquierda y derecha
            if (spriteTransform != null && Mathf.Abs(dir.x) > 0.15f)
            {
                Vector3 scale = spriteTransform.localScale;
                scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
                spriteTransform.localScale = scale;
            }
        }
 
        // La pose "Alert" solo se muestra cuando YA llegó al punto del ruido y está girando en el lugar,
        // no mientras todavía está caminando hacia allá (eso se ve como Walk normal).
        anim.SetBool(IsAlertHash, currentState == State.Alert && agent.isStopped);
 
        // Evita que el sprite rote en 3D junto con el EnemyRoot (que sí necesita rotar para el cono de visión).
        // Si tu Visual ya está desacoplado de otra forma, podés borrar esta línea.
        if (spriteTransform != null && spriteTransform != transform)
        {
            spriteTransform.rotation = Quaternion.identity;
        }
    }
    // ====================================================================
 

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