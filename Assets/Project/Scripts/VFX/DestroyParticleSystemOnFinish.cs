using UnityEngine;

/// <summary>
/// Destruye este GameObject cuando TODOS los ParticleSystem hijos (y el propio,
/// si lo tiene) han terminado de emitir y ya no quedan particulas vivas.
///
/// Pensado para VFX compuestos por varios sub-sistemas (ej: onda de choque +
/// fragmentos + flash de una bomba de sonido), donde usar el "Stop Action: Destroy"
/// nativo de un solo Particle System no sirve porque cada sub-sistema termina en
/// un momento distinto.
/// </summary>
[DisallowMultipleComponent]
public class DestroyParticleSystemOnFinish : MonoBehaviour
{
    [Tooltip("Tiempo de seguridad (segundos) tras el cual se fuerza la destruccion " +
             "aunque algun sistema siga reportando particulas vivas. Evita objetos " +
             "'fantasma' por bugs de sub-emitters o loops mal configurados.")]
    [SerializeField] private float safetyTimeout = 10f;

    [Tooltip("Si es true, revisa tambien los ParticleSystem en hijos inactivos.")]
    [SerializeField] private bool includeInactiveChildren = false;

    private ParticleSystem[] _systems;
    private float _elapsed;

    private void Awake()
    {
        _systems = GetComponentsInChildren<ParticleSystem>(includeInactiveChildren);

        if (_systems.Length == 0)
        {
            Debug.LogWarning(
                $"[{nameof(DestroyParticleSystemOnFinish)}] No se encontro ningun " +
                $"ParticleSystem en '{name}' ni en sus hijos. Destruyendo de inmediato.",
                this);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= safetyTimeout)
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < _systems.Length; i++)
        {
            if (_systems[i] != null && _systems[i].IsAlive(true))
            {
                return;
            }
        }

        Destroy(gameObject);
    }
}
