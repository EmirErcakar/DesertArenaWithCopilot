using UnityEngine;
using System.Collections;

/// <summary>
/// Boss enemy. Alternates melee charge and burst-fire ranged phases.
/// Boss death triggers the level-complete flow.
///
/// Inspector Setup:
///   - maxHealth: 500 (set on HealthComponent)
///   - moveSpeed: 3.5
///   - attackDamage: 20
///   - attackRange:  2.0
///   - attackCooldown: 0.8
///   - rangedPhaseDistance: 4.0
///   - phaseSwitchInterval: 5.0
///   - projectileSpeed: 6.0
///   - enemyProjectilePrefab: boss bullet prefab
///   - firePoint: muzzle Transform child
///   - xpReward: 100
///   - coinReward: 50
///
/// The Boss MUST be on the Enemy physics layer so TargetSelector prioritises it.
/// </summary>
public class BossEnemy : EnemyBase
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private float projectileSpeed      = 6f;
    [SerializeField] private float rangedPhaseDistance  = 4f;
    [SerializeField] private float phaseSwitchInterval  = 5f;

    private enum Phase { Melee, Ranged }
    private Phase currentPhase      = Phase.Melee;
    private float phaseSwitchTimer;
    private bool  deathSequencePlaying;

    protected override void Awake()
    {
        base.Awake();
        phaseSwitchTimer = phaseSwitchInterval;
    }

    protected override void UpdateBehaviour()
    {
        if (deathSequencePlaying) return;

        phaseSwitchTimer -= Time.deltaTime;
        if (phaseSwitchTimer <= 0f)
        {
            currentPhase     = currentPhase == Phase.Melee ? Phase.Ranged : Phase.Melee;
            phaseSwitchTimer = phaseSwitchInterval;
        }

        switch (currentPhase)
        {
            case Phase.Melee:  MeleePhase();  break;
            case Phase.Ranged: RangedPhase(); break;
        }
    }

    // ── Melee phase ──────────────────────────────

    void MeleePhase()
    {
        if (DistToPlayer > attackRange)
            MoveTowardsPlayer();
        else if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            player.GetComponent<HealthComponent>()?.TakeDamage(attackDamage);
        }
    }

    // ── Ranged phase ─────────────────────────────

    void RangedPhase()
    {
        if (DistToPlayer < rangedPhaseDistance)
        {
            Vector3 away = (transform.position - player.position).normalized;
            MoveTowards(transform.position + away * 2f);
        }
        else
        {
            FacePlayer();
        }

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            FireBurst();
        }
    }

    void FireBurst()
    {
        if (enemyProjectilePrefab == null || firePoint == null) return;

        // 3-projectile fan spread
        for (int i = -1; i <= 1; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * 15f, 0f)
                * (player.position - firePoint.position).normalized;
            dir.y = 0f;

            GameObject proj = Instantiate(enemyProjectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
            proj.GetComponent<Projectile>()?.Init(dir, projectileSpeed, attackRange * 2f, attackDamage * 0.5f);
        }
    }

    // ── Death ─────────────────────────────────────

    protected override void HandleDeath()
    {
        if (deathSequencePlaying) return;
        deathSequencePlaying = true;

        XPSystem.Instance?.AddXP(xpReward);
        CoinManager.Instance?.AddCoins(coinReward);
        WaveManager.Instance?.OnEnemyDied(this);

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Simple scale-down over 0.5 s then chest/level-end
        float elapsed   = 0f;
        float duration  = 0.5f;
        Vector3 origScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(origScale, Vector3.zero, elapsed / duration);
            yield return null;
        }

        // Trigger chest UI (boss always drops chest even after revives)
        ChestUI.Instance?.ShowChest();

        Destroy(gameObject);
    }
}
