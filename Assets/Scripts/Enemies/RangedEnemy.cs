using UnityEngine;

/// <summary>
/// Ranged enemy (joins ~65 s into Arena 1 Level 1).
/// Keeps a preferred distance from the player and fires projectiles.
///
/// Inspector Setup:
///   - moveSpeed:           2.0
///   - attackRange:         8.0  (projectile fire range)
///   - attackDamage:        6    (projectile damage)
///   - attackCooldown:      2.0
///   - preferredDistance:   5.0  (tries to stay at this distance)
///   - projectileSpeed:     7.0
///   - enemyProjectilePrefab: drag your enemy-projectile prefab here
///   - firePoint: an empty child Transform at the "muzzle" position
///   - xpReward:            18
///   - coinReward:          8
/// </summary>
public class RangedEnemy : EnemyBase
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private float projectileSpeed    = 7f;
    [SerializeField] private float preferredDistance  = 5f;

    protected override void UpdateBehaviour()
    {
        float dist = DistToPlayer;

        // Maintain preferred distance
        if (dist < preferredDistance * 0.75f)
        {
            // Too close — back away
            Vector3 awayDir = (transform.position - player.position).normalized;
            MoveTowards(transform.position + awayDir * 2f);
        }
        else if (dist > preferredDistance * 1.3f)
        {
            // Too far — close in
            MoveTowardsPlayer();
        }
        else
        {
            // Good distance — face player
            FacePlayer();
        }

        // Fire when in range and cooldown ready
        if (dist <= attackRange && attackTimer <= 0f)
            FireAtPlayer();
    }

    void FireAtPlayer()
    {
        attackTimer = attackCooldown;
        if (enemyProjectilePrefab == null || firePoint == null) return;

        Vector3 dir = (player.position - firePoint.position);
        dir.y = 0f;
        dir.Normalize();

        GameObject proj = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
        proj.GetComponent<Projectile>()?.Init(dir, projectileSpeed, attackRange * 1.5f, attackDamage);
    }
}
