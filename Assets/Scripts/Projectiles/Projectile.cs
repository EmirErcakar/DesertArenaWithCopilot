using UnityEngine;

namespace DesertArena.Projectiles
{
    /// <summary>
    /// A projectile that moves forward at a set speed, deals damage on contact,
    /// and self-destructs when exceeding max range or hitting an obstacle.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        /// <summary>
        /// Identifies who fired this projectile for tag-based hit detection.
        /// </summary>
        public enum ProjectileSource
        {
            Player,
            Enemy
        }

        #region Serialized Fields

        [Header("Projectile Settings")]
        [SerializeField] [Tooltip("Travel speed in units per second.")]
        private float speed = 20f;

        [SerializeField] [Tooltip("Damage dealt on hit.")]
        private float damage = 10f;

        [SerializeField] [Tooltip("Maximum distance before auto-destroy.")]
        private float maxRange = 25f;

        [SerializeField] [Tooltip("Layers this projectile can hit.")]
        private LayerMask hitLayerMask = ~0;

        [SerializeField] [Tooltip("Who fired this projectile.")]
        private ProjectileSource source = ProjectileSource.Player;

        #endregion

        #region Private Fields

        private Vector3 _direction;
        private float _distanceTraveled;
        private bool _initialized;

        #endregion

        #region Properties

        /// <summary>Source of this projectile (Player or Enemy).</summary>
        public ProjectileSource Source => source;

        /// <summary>Damage this projectile deals.</summary>
        public float Damage => damage;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the projectile with movement direction, speed, damage, and source.
        /// Call this after instantiation to configure behavior.
        /// </summary>
        /// <param name="direction">Normalized movement direction.</param>
        /// <param name="speed">Travel speed.</param>
        /// <param name="damage">Damage on hit.</param>
        /// <param name="source">Who fired this projectile.</param>
        public void Initialize(Vector3 direction, float speed, float damage,
            ProjectileSource source)
        {
            _direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.source = source;
            _distanceTraveled = 0f;
            _initialized = true;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            float moveDistance = speed * Time.deltaTime;
            transform.position += (_initialized ? _direction : transform.forward) * moveDistance;

            _distanceTraveled += moveDistance;
            if (_distanceTraveled >= maxRange)
            {
                DestroyProjectile();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.gameObject);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes a hit against another GameObject using tag-based detection.
        /// </summary>
        private void HandleHit(GameObject hitObject)
        {
            // Player projectiles damage enemies
            if (source == ProjectileSource.Player && hitObject.CompareTag("Enemy"))
            {
                Enemies.EnemyBase enemy = hitObject.GetComponent<Enemies.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                DestroyProjectile();
                return;
            }

            // Enemy projectiles damage the player
            if (source == ProjectileSource.Enemy && hitObject.CompareTag("Player"))
            {
                Player.PlayerStats playerStats =
                    hitObject.GetComponent<Player.PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damage);
                }

                DestroyProjectile();
                return;
            }

            // Hit an obstacle (anything that isn't the same team)
            if (!hitObject.CompareTag("Projectile"))
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Destroys or returns this projectile to the pool.
        /// </summary>
        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
