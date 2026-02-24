using System.Collections.Generic;
using UnityEngine;

namespace DesertArena.Projectiles
{
    /// <summary>
    /// Simple static object pool for projectiles. Pre-instantiates a configurable
    /// number of projectile GameObjects and recycles them to reduce garbage collection.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Pool Settings")]
        [SerializeField] [Tooltip("Projectile prefab to pool.")]
        private GameObject projectilePrefab;

        [SerializeField] [Tooltip("Number of projectiles to pre-instantiate.")]
        private int initialPoolSize = 30;

        #endregion

        #region Static Fields

        private static ProjectilePool _instance;
        private static Queue<GameObject> _pool = new Queue<GameObject>();
        private static List<GameObject> _activeProjectiles = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _pool = new Queue<GameObject>();
            _activeProjectiles = new List<GameObject>();

            PrewarmPool();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _pool.Clear();
                _activeProjectiles.Clear();
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets a projectile from the pool. If the pool is empty, recycles
        /// the oldest active projectile.
        /// </summary>
        /// <param name="position">World position for the projectile.</param>
        /// <param name="rotation">Rotation for the projectile.</param>
        /// <returns>An activated projectile GameObject, or null if no pool exists.</returns>
        public static GameObject Get(Vector3 position, Quaternion rotation)
        {
            if (_instance == null) return null;

            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();

                // Handle destroyed objects in the pool
                if (obj == null)
                {
                    obj = _instance.CreateProjectile();
                }
            }
            else
            {
                // Recycle oldest active projectile
                obj = RecycleOldest();
                if (obj == null)
                {
                    obj = _instance.CreateProjectile();
                }
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            _activeProjectiles.Add(obj);

            return obj;
        }

        /// <summary>
        /// Returns a projectile to the pool for reuse.
        /// </summary>
        /// <param name="projectile">The projectile GameObject to return.</param>
        public static void Return(GameObject projectile)
        {
            if (projectile == null) return;

            projectile.SetActive(false);
            _activeProjectiles.Remove(projectile);
            _pool.Enqueue(projectile);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Pre-instantiates projectiles and adds them to the pool.
        /// </summary>
        private void PrewarmPool()
        {
            if (projectilePrefab == null) return;

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = CreateProjectile();
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Instantiates a single projectile under this pool's transform.
        /// </summary>
        private GameObject CreateProjectile()
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Recycles the oldest active projectile when the pool is empty.
        /// </summary>
        private static GameObject RecycleOldest()
        {
            while (_activeProjectiles.Count > 0)
            {
                GameObject oldest = _activeProjectiles[0];
                _activeProjectiles.RemoveAt(0);

                if (oldest != null)
                {
                    oldest.SetActive(false);
                    return oldest;
                }
            }

            return null;
        }

        #endregion
    }
}
