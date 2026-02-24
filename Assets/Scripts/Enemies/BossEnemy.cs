using System.Collections;
using UnityEngine;
using DesertArena.Core;
using DesertArena.Projectiles;

namespace DesertArena.Enemies
{
    /// <summary>
    /// Boss enemy with a large health pool and multiple attack phases.
    /// HP does NOT reset when the player revives. On death, triggers a
    /// short death animation then signals level completion.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        #region Serialized Fields

        [Header("Boss Settings")]
        [SerializeField] [Tooltip("Seconds between melee attacks.")]
        private float meleeAttackCooldown = 1.5f;

        [SerializeField] [Tooltip("Projectile prefab for ranged phase.")]
        private GameObject projectilePrefab;

        [SerializeField] [Tooltip("Transform where projectiles spawn.")]
        private Transform firePoint;

        [SerializeField] [Tooltip("Speed of fired projectiles.")]
        private float projectileSpeed = 10f;

        [SerializeField] [Tooltip("Projectiles per second in ranged phase.")]
        private float fireRate = 2f;

        [SerializeField] [Tooltip("Duration of the death animation coroutine.")]
        private float deathAnimationDuration = 3.5f;

        #endregion

        #region Private Fields

        private float _attackTimer;
        private bool _isDying;

        /// <summary>Current attack phase based on remaining HP percentage.</summary>
        private int _currentPhase;

        #endregion

        #region Properties

        /// <summary>
        /// Current attack phase (1-3). Phase increases as HP drops.
        /// </summary>
        public int CurrentPhase => _currentPhase;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            enemySubType = EnemySubType.Boss;
            _currentPhase = 1;
        }

        protected override void Start()
        {
            base.Start();
            EventBus.RaiseBossSpawned();
        }

        protected override void Update()
        {
            if (_isDying || !IsAlive || playerTransform == null) return;

            // Tick cooldown
            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            UpdatePhase();

            float distSqr = DistanceToPlayerSqr();

            switch (_currentPhase)
            {
                case 1:
                    // Phase 1 (HP > 66%): simple melee chase
                    ChaseAndMelee(distSqr);
                    break;
                case 2:
                    // Phase 2 (33%-66%): melee + occasional ranged
                    ChaseAndMelee(distSqr);
                    TryFireProjectile();
                    break;
                case 3:
                    // Phase 3 (< 33%): aggressive melee + rapid ranged
                    ChaseAndMelee(distSqr);
                    TryFireProjectile();
                    break;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Boss death: fires events, plays death animation, then ends the level.
        /// </summary>
        public override void Die()
        {
            if (_isDying) return;
            _isDying = true;

            if (currentHP > 0f) currentHP = 0f;

            // Stop movement
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            // Fire events
            OnDeathInternal();

            StartCoroutine(DeathSequence());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the attack phase based on current HP percentage.
        /// </summary>
        private void UpdatePhase()
        {
            float hpPercent = currentHP / maxHP;

            if (hpPercent > 0.66f)
                _currentPhase = 1;
            else if (hpPercent > 0.33f)
                _currentPhase = 2;
            else
                _currentPhase = 3;
        }

        /// <summary>
        /// Chases the player and performs melee attacks when in range.
        /// </summary>
        private void ChaseAndMelee(float distSqr)
        {
            if (distSqr <= attackRange * attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                FacePlayer();

                if (_attackTimer <= 0f)
                {
                    float cooldown = _currentPhase == 3
                        ? meleeAttackCooldown * 0.6f
                        : meleeAttackCooldown;
                    _attackTimer = cooldown;

                    Player.PlayerStats playerStats =
                        playerTransform.GetComponent<Player.PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(attackDamage);
                    }
                }
            }
            else
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }

                MoveTowardPlayer();
            }
        }

        /// <summary>
        /// Fires projectiles during phases 2 and 3.
        /// </summary>
        private void TryFireProjectile()
        {
            if (projectilePrefab == null) return;
            if (_attackTimer > 0f) return;

            float rate = _currentPhase == 3 ? fireRate * 1.5f : fireRate;
            _attackTimer = rate > 0f ? 1f / rate : 1f;

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Vector3 direction = (playerTransform.position - spawnPoint.position).normalized;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;
            direction.Normalize();

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projObj = Instantiate(projectilePrefab, spawnPoint.position, rotation);

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, attackDamage * 0.5f,
                    Projectile.ProjectileSource.Enemy);
            }
        }

        /// <summary>
        /// Fires the OnDeath instance event, EventBus kill and boss-died events, and rewards.
        /// </summary>
        private void OnDeathInternal()
        {
            // Invoke the base class OnDeath event via a helper
            // (We can't invoke the base event directly, so we use EventBus)
            EventBus.RaiseEnemyKilled(EnemyType.Boss);
            EventBus.RaiseBossDied();
            EventBus.RaiseCoinCollected(coinReward);
            EventBus.RaiseXPGained(xpReward);
        }

        /// <summary>
        /// Coroutine that simulates a death animation, then triggers level completion.
        /// </summary>
        private IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(deathAnimationDuration);

            // Signal level end via GameManager
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetState(GameManager.GameState.Victory);
                GameManager.Instance.CompleteLevel();
            }

            Destroy(gameObject);
        }

        private void FacePlayer()
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        #endregion
    }
}
