using UnityEngine;

namespace DesertArena.Player
{
    /// <summary>
    /// Finds the best target for the player using threat-priority logic.
    /// Priority: Boss > Nearest Ranged enemy > Nearest enemy overall.
    /// Updates at a fixed interval to save performance.
    /// If LayerMask is not set, falls back to tag-based ("Enemy") detection.
    /// </summary>
    public class PlayerTargeting : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Detection")]
        [SerializeField]
        [Tooltip("Radius of the overlap sphere used to detect enemies.")]
        private float _detectionRange = 15f;

        [SerializeField]
        [Tooltip("Layer mask for enemy objects (0 = tag tabanlı otomatik tespit).")]
        private LayerMask _enemyLayerMask;

        [Header("Performance")]
        [SerializeField]
        [Tooltip("How often (seconds) the targeting scan runs.")]
        private float _updateInterval = 0.2f;

        [SerializeField]
        [Tooltip("Maximum number of colliders the scan can process.")]
        private int _maxDetectedEnemies = 50;

        #endregion

        #region Private Fields

        private Transform _currentTarget;
        private float _updateTimer;
        private Collider[] _hitBuffer;
        private bool _useTagFallback;

        #endregion

        #region Properties

        /// <summary>The current best target, or null if none found.</summary>
        public Transform CurrentTarget => _currentTarget;

        /// <summary>Detection range for editor gizmos and external queries.</summary>
        public float DetectionRange => _detectionRange;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _hitBuffer = new Collider[_maxDetectedEnemies];

            // LayerMask 0 ise tag tabanlı tespit kullan
            if (_enemyLayerMask.value == 0)
            {
                _useTagFallback = true;
                // Tüm layer'ları tara, sonra tag ile filtrele
                _enemyLayerMask = ~0;
                Debug.Log("[PlayerTargeting] LayerMask atanmamış — 'Enemy' tag ile otomatik tespit aktif");
            }
        }

        private void Update()
        {
            _updateTimer -= Time.deltaTime;
            if (_updateTimer <= 0f)
            {
                _updateTimer = _updateInterval;
                UpdateTarget();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }

        #endregion

        #region Private Methods

        private void UpdateTarget()
        {
            if (_useTagFallback)
            {
                UpdateTargetByTag();
                return;
            }

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position, _detectionRange, _hitBuffer, _enemyLayerMask);

            if (hitCount == 0)
            {
                _currentTarget = null;
                return;
            }

            ProcessHits(hitCount);
        }

        /// <summary>
        /// Tag tabanlı düşman tespiti — LayerMask ayarlanmadığında kullanılır.
        /// </summary>
        private void UpdateTargetByTag()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies == null || enemies.Length == 0)
            {
                _currentTarget = null;
                return;
            }

            Transform bestBoss = null;
            float bestBossDist = float.MaxValue;

            Transform bestRanged = null;
            float bestRangedDist = float.MaxValue;

            Transform bestAny = null;
            float bestAnyDist = float.MaxValue;

            Vector3 myPos = transform.position;
            float rangeSqr = _detectionRange * _detectionRange;

            foreach (GameObject enemyObj in enemies)
            {
                if (enemyObj == null) continue;

                Transform enemyTransform = enemyObj.transform;
                float distSqr = Vector3.SqrMagnitude(enemyTransform.position - myPos);

                // Menzil dışındaysa atla
                if (distSqr > rangeSqr) continue;

                var enemyTag = enemyObj.GetComponent<EnemyTag>();
                var enemyBase = enemyObj.GetComponent<Enemies.EnemyBase>();

                // Ölü düşmanları atla
                if (enemyBase != null && !enemyBase.IsAlive) continue;

                if (enemyTag != null)
                {
                    if (enemyTag.Type == Core.EnemyType.Boss && distSqr < bestBossDist)
                    {
                        bestBoss = enemyTransform;
                        bestBossDist = distSqr;
                    }
                    else if (enemyTag.Type == Core.EnemyType.Ranged && distSqr < bestRangedDist)
                    {
                        bestRanged = enemyTransform;
                        bestRangedDist = distSqr;
                    }
                }

                if (distSqr < bestAnyDist)
                {
                    bestAny = enemyTransform;
                    bestAnyDist = distSqr;
                }
            }

            // Priority: Boss > Ranged > Nearest
            if (bestBoss != null)
                _currentTarget = bestBoss;
            else if (bestRanged != null)
                _currentTarget = bestRanged;
            else
                _currentTarget = bestAny;
        }

        private void ProcessHits(int hitCount)
        {
            Transform bestBoss = null;
            float bestBossDist = float.MaxValue;

            Transform bestRanged = null;
            float bestRangedDist = float.MaxValue;

            Transform bestAny = null;
            float bestAnyDist = float.MaxValue;

            Vector3 myPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null) continue;

                // Tag tabanlı filtreleme (fallback modunda da çalışır)
                if (_useTagFallback && !col.CompareTag("Enemy")) continue;

                Transform enemyTransform = col.transform;
                float distSqr = Vector3.SqrMagnitude(enemyTransform.position - myPos);

                // Check for enemy tag component to determine type
                var enemyTag = col.GetComponent<EnemyTag>();

                if (enemyTag != null)
                {
                    if (enemyTag.Type == Core.EnemyType.Boss && distSqr < bestBossDist)
                    {
                        bestBoss = enemyTransform;
                        bestBossDist = distSqr;
                    }
                    else if (enemyTag.Type == Core.EnemyType.Ranged && distSqr < bestRangedDist)
                    {
                        bestRanged = enemyTransform;
                        bestRangedDist = distSqr;
                    }
                }

                if (distSqr < bestAnyDist)
                {
                    bestAny = enemyTransform;
                    bestAnyDist = distSqr;
                }
            }

            // Priority: Boss > Ranged > Nearest
            if (bestBoss != null)
                _currentTarget = bestBoss;
            else if (bestRanged != null)
                _currentTarget = bestRanged;
            else
                _currentTarget = bestAny;
        }

        #endregion
    }

    /// <summary>
    /// Lightweight component placed on enemy GameObjects to identify their type.
    /// Used by PlayerTargeting for threat-priority selection.
    /// </summary>
    public class EnemyTag : MonoBehaviour
    {
        [SerializeField] private Core.EnemyType _enemyType = Core.EnemyType.Melee;

        /// <summary>The archetype of this enemy.</summary>
        public Core.EnemyType Type => _enemyType;
    }
}
