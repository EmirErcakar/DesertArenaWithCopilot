using UnityEngine;

namespace DesertArena.Enemies
{
    /// <summary>
    /// Melee enemy that chases the player and attacks at close range.
    /// Knife variant uses short range (1.5f), Sword variant uses longer range (2.5f).
    /// </summary>
    public class MeleeEnemy : EnemyBase
    {
        #region Serialized Fields

        [Header("Melee Settings")]
        [SerializeField] [Tooltip("Seconds between attacks.")]
        private float attackCooldown = 1.2f;

        #endregion

        #region Private Fields

        private float _attackTimer;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            // Set attack range based on sub-type
            if (enemySubType == EnemySubType.MeleeKnife)
            {
                attackRange = 1.5f;
            }
            else if (enemySubType == EnemySubType.MeleeSword)
            {
                attackRange = 2.5f;
            }
        }

        protected override void Update()
        {
            if (!IsAlive || playerTransform == null) return;

            // Tick cooldown
            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            float distSqr = DistanceToPlayerSqr();

            if (distSqr <= attackRange * attackRange)
            {
                // In range – stop and attack
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                FacePlayer();
                TryAttack();
            }
            else
            {
                // Chase the player
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }

                MoveTowardPlayer();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempts a melee attack if the cooldown has elapsed.
        /// </summary>
        private void TryAttack()
        {
            if (_attackTimer > 0f) return;

            _attackTimer = attackCooldown;

            // Deal damage to the player
            Player.PlayerStats playerStats = playerTransform.GetComponent<Player.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
            }
        }

        /// <summary>
        /// Rotates the enemy to face the player.
        /// </summary>
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
