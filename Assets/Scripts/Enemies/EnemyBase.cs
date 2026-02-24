using UnityEngine;

/// <summary>
/// Abstract base class for all enemy types.
/// Handles shared behaviour: finding the player, movement, death rewards.
///
/// Inspector Setup (on every enemy prefab):
///   - moveSpeed:       movement speed (units/sec)
///   - attackDamage:    damage per hit
///   - attackRange:     distance at which the unit can attack
///   - attackCooldown:  seconds between attacks
///   - xpReward:        XP granted to player on death
///   - coinReward:      coins granted to player on death
///
/// Requires: HealthComponent on same GameObject
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float moveSpeed      = 2.5f;
    [SerializeField] protected float attackDamage   = 10f;
    [SerializeField] protected float attackRange    = 1.2f;
    [SerializeField] protected float attackCooldown = 1f;

    [Header("Rewards")]
    [SerializeField] protected int xpReward   = 10;
    [SerializeField] protected int coinReward = 5;

    protected HealthComponent health;
    protected Transform        player;
    protected float            attackTimer;

    // ──────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────

    protected virtual void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.OnDeath += HandleDeath;
    }

    protected virtual void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (health.IsDead || player == null) return;

        attackTimer -= Time.deltaTime;
        UpdateBehaviour();
    }

    // ──────────────────────────────────────────────
    // Subclass contract
    // ──────────────────────────────────────────────

    /// <summary>Called every frame while alive and game is playing.</summary>
    protected abstract void UpdateBehaviour();

    // ──────────────────────────────────────────────
    // Shared helpers
    // ──────────────────────────────────────────────

    protected float DistToPlayer =>
        player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

    protected void MoveTowardsPlayer()
    {
        if (player == null) return;
        MoveTowards(player.position);
    }

    protected void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        dir.Normalize();
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation  = Quaternion.LookRotation(dir);
    }

    protected void FacePlayer()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // ──────────────────────────────────────────────
    // Death
    // ──────────────────────────────────────────────

    protected virtual void HandleDeath()
    {
        XPSystem.Instance?.AddXP(xpReward);
        CoinManager.Instance?.AddCoins(coinReward);
        WaveManager.Instance?.OnEnemyDied(this);
        Destroy(gameObject, 0.05f);
    }
}
