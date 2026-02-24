using UnityEngine;

/// <summary>
/// Knife-wielding melee enemy (first 35 s of Arena 1 Level 1).
/// Chases the player and attacks at close range.
///
/// Inspector Setup:
///   - moveSpeed:      2.5
///   - attackRange:    1.0
///   - attackDamage:   8
///   - attackCooldown: 1.0
///   - xpReward:       8
///   - coinReward:     3
/// </summary>
public class MeleeEnemy : EnemyBase
{
    protected override void UpdateBehaviour()
    {
        if (DistToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else if (attackTimer <= 0f)
        {
            Attack();
        }
    }

    void Attack()
    {
        attackTimer = attackCooldown;
        player.GetComponent<HealthComponent>()?.TakeDamage(attackDamage);
    }
}
