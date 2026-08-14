using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float lifetime = 10f;

    [Header("Ruido")]
    [SerializeField] private float hearingRadius = 8f;      // Qué tan lejos alcanzan a "escuchar" los enemigos
    [SerializeField] private LayerMask enemyLayerMask;       // Layer enemigos

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
            Explode();
    }

    public void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        AlertNearbyEnemies(); 
        Destroy(gameObject);
    }

    private void AlertNearbyEnemies()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, hearingRadius, enemyLayerMask);
        foreach (Collider col in enemiesInRange)
        {
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.HearNoise(transform.position);
            }
        }
    }
}
