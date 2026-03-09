using UnityEngine;
using DesertArena.Projectiles;

namespace DesertArena.Enemies
{
    /// <summary>
    /// Ranged enemy that approaches the player but maintains a preferred distance,
    /// then fires projectiles on a cooldown.
    /// </summary>
    public class RangedEnemy : EnemyBase
    {
        #region Serialized Fields

        [Header("Ranged Settings")]
        [SerializeField] [Tooltip("Projectile prefab to fire.")]
        private GameObject projectilePrefab;

        [SerializeField] [Tooltip("Projectiles per second.")]
        private float fireRate = 1f;

        [SerializeField] [Tooltip("Minimum preferred distance from player.")]
        private float preferredDistanceMin = 5f;

        [SerializeField] [Tooltip("Maximum preferred distance from player.")]
        private float preferredDistanceMax = 7f;

        [SerializeField] [Tooltip("Transform where projectiles spawn.")]
        private Transform firePoint;

        [SerializeField] [Tooltip("Speed of fired projectiles.")]
        private float projectileSpeed = 12f;

        #endregion

        #region Private Fields

        private float _fireCooldown;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            enemySubType = EnemySubType.Ranged;
        }

        protected override void Update()
        {
            if (!IsAlive || playerTransform == null) return;

            // Tick fire cooldown
            if (_fireCooldown > 0f)
            {
                _fireCooldown -= Time.deltaTime;
            }

            float dist = Vector3.Distance(playerTransform.position, transform.position);

            if (dist > preferredDistanceMax)
            {
                // Too far – approach
                if (!_useSimpleMovement && agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else if (_useSimpleMovement)
                {
                    Vector3 dir = (playerTransform.position - transform.position).normalized;
                    dir.y = 0f;
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }
            }
            else if (dist < preferredDistanceMin)
            {
                // Too close – back away
                if (!_useSimpleMovement && agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    Vector3 awayDir = (transform.position - playerTransform.position).normalized;
                    Vector3 retreatPos = transform.position + awayDir * 2f;
                    agent.SetDestination(retreatPos);
                }
                else if (_useSimpleMovement)
                {
                    Vector3 awayDir = (transform.position - playerTransform.position).normalized;
                    awayDir.y = 0f;
                    transform.position += awayDir * moveSpeed * Time.deltaTime;
                }
            }
            else
            {
                // In preferred zone – stop moving
                if (!_useSimpleMovement && agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
            }

            FacePlayer();
            TryFire();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Fires a projectile at the player if the cooldown has elapsed.
        /// </summary>
        private void TryFire()
        {
            if (_fireCooldown > 0f) return;
            if (projectilePrefab == null) return;

            _fireCooldown = fireRate > 0f ? 1f / fireRate : 1f;

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Vector3 direction = (playerTransform.position - spawnPoint.position).normalized;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;
            direction.Normalize();

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projObj = ProjectilePool.Get(spawnPoint.position, rotation);
            if (projObj == null)
            {
                projObj = Instantiate(projectilePrefab, spawnPoint.position, rotation);
            }

            Projectile proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, attackDamage, Projectile.ProjectileSource.Enemy);
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
