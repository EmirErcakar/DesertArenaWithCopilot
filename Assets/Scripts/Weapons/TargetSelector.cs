using UnityEngine;

/// <summary>
/// Selects the best attack target using threat-priority logic.
/// Priority order: Boss > Ranged enemy > Nearest enemy.
///
/// Inspector Setup:
///   - Attach to the Player GameObject alongside WeaponController
///   - detectionRadius: should match weaponRange in WeaponController (default 12)
///   - enemyLayer: set to the "Enemy" physics layer
/// </summary>
public class TargetSelector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private LayerMask enemyLayer;

    /// <summary>The current highest-priority target, or null if none in range.</summary>
    public Transform CurrentTarget { get; private set; }

    // Pre-allocated buffer avoids per-frame allocation
    private static readonly Collider[] _hitBuffer = new Collider[32];

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            CurrentTarget = null;
            return;
        }
        CurrentTarget = FindBestTarget();
    }

    Transform FindBestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectionRadius, _hitBuffer, enemyLayer);

        if (count == 0) return null;

        Transform boss      = null;
        float     bossDist  = float.MaxValue;
        Transform ranged    = null;
        float     rangedDist= float.MaxValue;
        Transform nearest   = null;
        float     nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = _hitBuffer[i];
            if (col == null) continue;

            var health = col.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);

            // Priority 1: Boss
            if (col.GetComponent<BossEnemy>() != null)
            {
                if (dist < bossDist) { bossDist = dist; boss = col.transform; }
                continue;
            }

            // Priority 2: Ranged enemy
            if (col.GetComponent<RangedEnemy>() != null)
            {
                if (dist < rangedDist) { rangedDist = dist; ranged = col.transform; }
                continue;
            }

            // Priority 3: Nearest melee enemy
            if (dist < nearestDist) { nearestDist = dist; nearest = col.transform; }
        }

        if (boss   != null) return boss;
        if (ranged != null) return ranged;
        return nearest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
