using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DesertArena.Core;
using DesertArena.Player;

namespace DesertArena.Enemies
{
    /// <summary>
    /// Manages wave-based enemy spawning for a level.
    /// Spawns enemies in phases over time, with difficulty ramping.
    /// Boss spawns on every 5th level with a countdown timer.
    /// Auto-creates spawn points and basic enemy prefabs if not assigned.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Spawn Points")]
        [SerializeField] [Tooltip("Transforms where enemies can be spawned (otomatik oluşturulur).")]
        private Transform[] spawnPoints;

        [Header("Enemy Prefabs")]
        [SerializeField] [Tooltip("Melee knife enemy prefab (otomatik oluşturulur).")]
        private GameObject meleeKnifePrefab;

        [SerializeField] [Tooltip("Melee sword enemy prefab (otomatik oluşturulur).")]
        private GameObject meleeSwordPrefab;

        [SerializeField] [Tooltip("Ranged enemy prefab (otomatik oluşturulur).")]
        private GameObject rangedEnemyPrefab;

        [SerializeField] [Tooltip("Boss enemy prefab (otomatik oluşturulur).")]
        private GameObject bossPrefab;

        [Header("Spawn Settings")]
        [SerializeField] [Tooltip("Maximum number of enemies alive at once.")]
        private int maxConcurrentEnemies = 15;

        [SerializeField] [Tooltip("Base time between spawns (seconds).")]
        private float baseSpawnInterval = 2f;

        [SerializeField] [Tooltip("Minimum spawn interval as difficulty ramps.")]
        private float minSpawnInterval = 0.5f;

        [SerializeField] [Tooltip("How much faster spawns get per second elapsed.")]
        private float spawnAcceleration = 0.01f;

        [Header("Phase Timing (seconds)")]
        [SerializeField] [Tooltip("Time when Phase 2 begins (MeleeSword added).")]
        private float phase2StartTime = 35f;

        [SerializeField] [Tooltip("Time when Phase 3 begins (Ranged added).")]
        private float phase3StartTime = 65f;

        [Header("Boss")]
        [SerializeField] [Tooltip("Boss countdown duration in seconds.")]
        private int bossCountdownDuration = 10;

        [Header("Auto-Setup")]
        [SerializeField] [Tooltip("Arena radius for auto-generated spawn points.")]
        private float arenaRadius = 20f;

        #endregion

        #region Private Fields

        private float _elapsedTime;
        private float _spawnTimer;
        private int _currentEnemyCount;
        private bool _bossSpawned;
        private bool _spawningActive;

        #endregion

        #region Properties

        /// <summary>Elapsed time since spawning started.</summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>Number of enemies currently alive.</summary>
        public int CurrentEnemyCount => _currentEnemyCount;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            EventBus.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void Start()
        {
            // Spawn noktaları yoksa otomatik oluştur
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                CreateAutoSpawnPoints();
            }

            // Prefab'lar atanmadıysa otomatik oluştur
            EnsureEnemyPrefabs();

            _spawningActive = true;

            // Check if this is a boss level (every 5th level)
            if (GameManager.HasInstance && (GameManager.Instance.CurrentLevel + 1) % 5 == 0)
            {
                StartCoroutine(BossCountdownRoutine());
            }
        }

        private void Update()
        {
            if (!_spawningActive || _bossSpawned) return;

            _elapsedTime += Time.deltaTime;

            // Spawn timer
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = CurrentSpawnInterval();
                TrySpawnEnemy();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stops all spawning activity.
        /// </summary>
        public void StopSpawning()
        {
            _spawningActive = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the current spawn interval, which decreases over time.
        /// </summary>
        private float CurrentSpawnInterval()
        {
            float interval = baseSpawnInterval - (_elapsedTime * spawnAcceleration);
            return Mathf.Max(interval, minSpawnInterval);
        }

        /// <summary>
        /// Attempts to spawn an enemy at a random spawn point.
        /// </summary>
        private void TrySpawnEnemy()
        {
            if (_currentEnemyCount >= maxConcurrentEnemies) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            GameObject prefab = ChooseEnemyPrefab();
            if (prefab == null) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPos = point.position;

            // NavMesh varsa en yakın geçerli noktayı bul
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            GameObject enemyObj = Instantiate(prefab, spawnPos, point.rotation);
            enemyObj.transform.SetParent(null);
            enemyObj.SetActive(true);

            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.OnDeath += HandleEnemyDeath;
            }

            _currentEnemyCount++;
        }

        /// <summary>
        /// Selects an enemy prefab based on the current phase.
        /// </summary>
        private GameObject ChooseEnemyPrefab()
        {
            if (_elapsedTime < phase2StartTime)
            {
                // Phase 1: only MeleeKnife
                return meleeKnifePrefab;
            }
            else if (_elapsedTime < phase3StartTime)
            {
                // Phase 2: MeleeKnife + MeleeSword
                return Random.value < 0.5f ? meleeKnifePrefab : meleeSwordPrefab;
            }
            else
            {
                // Phase 3: MeleeKnife + MeleeSword + Ranged
                float roll = Random.value;
                if (roll < 0.35f) return meleeKnifePrefab;
                if (roll < 0.65f) return meleeSwordPrefab;
                return rangedEnemyPrefab;
            }
        }

        /// <summary>
        /// Handles an individual enemy's OnDeath event to decrement the count.
        /// </summary>
        private void HandleEnemyDeath(EnemyBase enemy)
        {
            enemy.OnDeath -= HandleEnemyDeath;
            _currentEnemyCount = Mathf.Max(_currentEnemyCount - 1, 0);
        }

        /// <summary>
        /// Handles the EventBus enemy killed event (for external tracking).
        /// </summary>
        private void HandleEnemyKilled(EnemyType type)
        {
            // Reserved for analytics or UI updates
        }

        /// <summary>
        /// Coroutine that displays a boss countdown, spawns the boss,
        /// and stops regular enemy spawning.
        /// </summary>
        private IEnumerator BossCountdownRoutine()
        {
            // Let regular enemies spawn for a bit first
            yield return new WaitForSeconds(phase2StartTime);

            // Stop regular spawning
            _spawningActive = false;

            // Set game state to BossCountdown
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetState(GameManager.GameState.BossCountdown);
            }

            // Countdown
            EventBus.RaiseBossCountdownStarted(bossCountdownDuration);
            for (int i = bossCountdownDuration; i > 0; i--)
            {
                Debug.Log($"BIG BOSS SPAWNING IN {i}...");
                yield return new WaitForSeconds(1f);
            }

            // Resume playing state
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            }

            SpawnBoss();
        }

        /// <summary>
        /// Spawns the boss enemy at a random spawn point.
        /// </summary>
        private void SpawnBoss()
        {
            if (bossPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

            _bossSpawned = true;
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(bossPrefab, point.position, point.rotation);
        }

        /// <summary>
        /// Arena etrafında otomatik spawn noktaları oluşturur.
        /// </summary>
        private void CreateAutoSpawnPoints()
        {
            int pointCount = 5;
            spawnPoints = new Transform[pointCount];

            GameObject container = new GameObject("SpawnPoints (Auto)");
            container.transform.SetParent(transform);

            float spawnZ = arenaRadius;
            float spreadX = arenaRadius * 0.8f;

            for (int i = 0; i < pointCount; i++)
            {
                float t = pointCount > 1 ? (float)i / (pointCount - 1) : 0.5f;
                float x = Mathf.Lerp(-spreadX, spreadX, t);
                Vector3 pos = new Vector3(x, 0f, spawnZ);

                GameObject point = new GameObject($"SpawnPoint_{i}");
                point.transform.SetParent(container.transform);
                point.transform.position = pos;
                spawnPoints[i] = point.transform;
            }

            Debug.Log($"[EnemySpawner] {pointCount} spawn noktası üst kenardan oluşturuldu (Z={spawnZ})");
        }

        /// <summary>
        /// Prefab'lar atanmadıysa basit düşman prefab'ları oluşturur.
        /// </summary>
        private void EnsureEnemyPrefabs()
        {
            if (meleeKnifePrefab == null)
            {
                meleeKnifePrefab = CreateBasicEnemyPrefab("MeleeKnife_Auto",
                    EnemySubType.MeleeKnife, new Color(0.8f, 0.2f, 0.2f));
                Debug.Log("[EnemySpawner] MeleeKnife prefab otomatik oluşturuldu");
            }

            if (meleeSwordPrefab == null)
            {
                meleeSwordPrefab = CreateBasicEnemyPrefab("MeleeSword_Auto",
                    EnemySubType.MeleeSword, new Color(0.8f, 0.5f, 0.2f));
                Debug.Log("[EnemySpawner] MeleeSword prefab otomatik oluşturuldu");
            }

            if (rangedEnemyPrefab == null)
            {
                rangedEnemyPrefab = CreateBasicRangedPrefab("RangedEnemy_Auto",
                    new Color(0.2f, 0.2f, 0.8f));
                Debug.Log("[EnemySpawner] RangedEnemy prefab otomatik oluşturuldu");
            }

            // Boss prefab sadece boss seviyelerinde gerekli
            if (bossPrefab == null)
            {
                bossPrefab = CreateBasicBossPrefab("BossEnemy_Auto",
                    new Color(0.6f, 0.1f, 0.6f));
                Debug.Log("[EnemySpawner] BossEnemy prefab otomatik oluşturuldu");
            }
        }

        /// <summary>
        /// Basit bir melee düşman prefab'ı oluşturur (Capsule şeklinde).
        /// </summary>
        private GameObject CreateBasicEnemyPrefab(string name, EnemySubType subType, Color color)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = name;
            prefab.tag = "Enemy";
            prefab.SetActive(false);

            // Renk ata
            Renderer rend = prefab.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = CreateColorMaterial(color);
            }

            // Collider'ı trigger yap (fizik çarpışmaları için)
            CapsuleCollider col = prefab.GetComponent<CapsuleCollider>();
            if (col != null) col.isTrigger = false;

            // Rigidbody ekle (mermi trigger çarpışması için gerekli)
            Rigidbody rb = prefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            // MeleeEnemy bileşenini ekle
            MeleeEnemy melee = prefab.AddComponent<MeleeEnemy>();
            // EnemySubType ayarlamak için reflection kullan (private field)
            SetEnemySubType(melee, subType);

            // EnemyTag bileşeni ekle
            EnemyTag tag = prefab.AddComponent<EnemyTag>();
            SetEnemyTagType(tag, subType == EnemySubType.MeleeKnife || subType == EnemySubType.MeleeSword
                ? EnemyType.Melee : EnemyType.Ranged);

            DontDestroyOnLoad(prefab);

            return prefab;
        }

        /// <summary>
        /// Basit bir ranged düşman prefab'ı oluşturur.
        /// </summary>
        private GameObject CreateBasicRangedPrefab(string name, Color color)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = name;
            prefab.tag = "Enemy";
            prefab.SetActive(false);

            Renderer rend = prefab.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = CreateColorMaterial(color);
            }

            RangedEnemy ranged = prefab.AddComponent<RangedEnemy>();
            SetEnemySubType(ranged, EnemySubType.Ranged);

            // Rigidbody ekle (mermi trigger çarpışması için gerekli)
            Rigidbody rb = prefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            EnemyTag tag = prefab.AddComponent<EnemyTag>();
            SetEnemyTagType(tag, EnemyType.Ranged);

            DontDestroyOnLoad(prefab);

            return prefab;
        }

        /// <summary>
        /// Basit bir boss düşman prefab'ı oluşturur.
        /// </summary>
        private GameObject CreateBasicBossPrefab(string name, Color color)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = name;
            prefab.tag = "Enemy";
            prefab.SetActive(false);

            // Boss daha büyük olsun
            prefab.transform.localScale = new Vector3(2f, 2f, 2f);

            Renderer rend = prefab.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = CreateColorMaterial(color);
            }

            BossEnemy boss = prefab.AddComponent<BossEnemy>();
            SetEnemySubType(boss, EnemySubType.Boss);

            // Rigidbody ekle (mermi trigger çarpışması için gerekli)
            Rigidbody rb = prefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            EnemyTag tag = prefab.AddComponent<EnemyTag>();
            SetEnemyTagType(tag, EnemyType.Boss);

            DontDestroyOnLoad(prefab);

            return prefab;
        }

        /// <summary>
        /// EnemyBase'deki protected enemySubType alanını ayarlar.
        /// </summary>
        private void SetEnemySubType(EnemyBase enemy, EnemySubType subType)
        {
            var field = typeof(EnemyBase).GetField("enemySubType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(enemy, subType);
        }

        /// <summary>
        /// EnemyTag'deki private _enemyType alanını ayarlar.
        /// </summary>
        private void SetEnemyTagType(EnemyTag tag, EnemyType type)
        {
            var field = typeof(EnemyTag).GetField("_enemyType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(tag, type);
        }

        /// <summary>
        /// Belirtilen renkte bir Material oluşturur. URP shader yoksa Standard kullanır.
        /// </summary>
        private static Material CreateColorMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        #endregion
    }
}
