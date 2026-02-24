using UnityEngine;

/// <summary>
/// Moves forward and destroys on: enemy hit, obstacle hit, or range consumed.
///
/// Inspector Setup:
///   - Attach to a Projectile prefab
///   - Add a Collider (e.g., SphereCollider, isTrigger = true)
///   - Add a Rigidbody (Is Kinematic = true, no gravity)
///   - enemyLayer: set to the "Enemy" physics layer
///   - obstacleLayer: set to the "Obstacle" physics layer
///
/// Physics Layer setup in Unity (Edit > Project Settings > Physics):
///   - Create layers: Default(0), Player(6), Enemy(7), Obstacle(8), PlayerProjectile(9), EnemyProjectile(10)
///   - Layer collision matrix: PlayerProjectile collides with Enemy + Obstacle only
///   - EnemyProjectile collides with Player + Obstacle only
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Vector3 direction;
    private float   speed;
    private float   remainingRange;
    private float   damage;
    private bool    initialized;

    /// <summary>Called by WeaponController (or RangedEnemy) immediately after instantiation.</summary>
    public void Init(Vector3 dir, float spd, float range, float dmg)
    {
        direction      = dir.normalized;
        speed          = spd;
        remainingRange = range;
        damage         = dmg;
        initialized    = true;
    }

    void Update()
    {
        if (!initialized) return;

        float dist = speed * Time.deltaTime;
        transform.position += direction * dist;
        remainingRange -= dist;

        if (remainingRange <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        int otherLayer = 1 << other.gameObject.layer;

        // Hit obstacle -> destroy projectile
        if ((otherLayer & obstacleLayer) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // Hit enemy -> deal damage -> destroy projectile
        if ((otherLayer & enemyLayer) != 0)
        {
            var health = other.GetComponent<HealthComponent>();
            if (health != null && !health.IsDead)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
