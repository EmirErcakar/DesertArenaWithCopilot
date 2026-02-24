using UnityEngine;

/// <summary>
/// Sword-wielding melee enemy (joins ~35 s into Arena 1 Level 1).
/// Slightly longer attack reach and higher damage than MeleeEnemy.
///
/// Inspector Setup:
///   - moveSpeed:      2.2
///   - attackRange:    1.8  (longer reach than knife)
///   - attackDamage:   12
///   - attackCooldown: 1.2
///   - xpReward:       12
///   - coinReward:     5
/// </summary>
public class SwordEnemy : EnemyBase
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
