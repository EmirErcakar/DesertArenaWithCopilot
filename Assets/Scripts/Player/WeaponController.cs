using UnityEngine;
using DesertArena.Projectiles;

namespace DesertArena.Player
{
    /// <summary>
    /// Handles automatic projectile firing toward the current target.
    /// Fire rate is governed by PlayerStats.AttackSpeed.
    /// Auto-creates a projectile prefab if none is assigned.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Projectile")]
        [SerializeField]
        [Tooltip("Prefab instantiated each time the weapon fires (otomatik oluşturulur).")]
        private GameObject _projectilePrefab;

        [SerializeField]
        [Tooltip("Spawn point for projectiles (e.g., muzzle transform).")]
        private Transform _firePoint;

        [Header("Weapon Data")]
        [SerializeField]
        [Tooltip("Maximum range of the weapon.")]
        private float _range = 12f;

        [SerializeField]
        [Tooltip("Base damage per projectile (scaled by PlayerStats.AttackDamage).")]
        private float _baseDamage = 10f;

        [SerializeField]
        [Tooltip("Projectile travel speed.")]
        private float _projectileSpeed = 20f;

        #endregion

        #region Private Fields

        private PlayerStats _stats;
        private PlayerTargeting _targeting;
        private float _fireCooldown;

        #endregion

        #region Properties

        /// <summary>Effective weapon range.</summary>
        public float Range => _range;

        /// <summary>Damage per projectile after stat scaling.</summary>
        public float Damage => _baseDamage + (_stats != null ? _stats.AttackDamage : 0f);

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _targeting = GetComponent<PlayerTargeting>();
        }

        private void Start()
        {
            // Projectile prefab yoksa otomatik oluştur
            if (_projectilePrefab == null)
            {
                _projectilePrefab = CreateAutoProjectilePrefab();
                Debug.Log("[WeaponController] Mermi prefab'ı otomatik oluşturuldu");
            }
        }

        private void Update()
        {
            if (_fireCooldown > 0f)
            {
                _fireCooldown -= Time.deltaTime;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to fire a projectile toward the current target.
        /// Respects the fire-rate cooldown derived from AttackSpeed.
        /// </summary>
        public void TryFire()
        {
            if (_fireCooldown > 0f) return;
            if (_projectilePrefab == null) return;

            Transform target = _targeting != null ? _targeting.CurrentTarget : null;
            if (target == null) return;

            // Check range
            float distSqr = Vector3.SqrMagnitude(target.position - transform.position);
            if (distSqr > _range * _range) return;

            // Calculate fire interval from attack speed (attacks per second)
            float attackSpeed = _stats != null ? _stats.AttackSpeed : 1f;
            _fireCooldown = attackSpeed > 0f ? 1f / attackSpeed : 1f;

            SpawnProjectile(target);
        }

        #endregion

        #region Private Methods

        private void SpawnProjectile(Transform target)
        {
            Transform spawnPoint = _firePoint != null ? _firePoint : transform;

            Vector3 direction = target.position - spawnPoint.position;
            direction.y = 0f;
            direction.Normalize();

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPoint.position, rotation);

            // Projectile bileşenini bul ve Initialize et
            Projectile proj = projectileObj.GetComponent<Projectile>();
            if (proj != null)
            {
                float totalDamage = _baseDamage + (_stats != null ? _stats.AttackDamage : 0f);
                proj.Initialize(direction, _projectileSpeed, totalDamage,
                    Projectile.ProjectileSource.Player);
            }
            else
            {
                // Projectile bileşeni yoksa Rigidbody ile hareket et
                if (projectileObj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = direction * _projectileSpeed;
                }

                // Menzil sonunda yok et
                float lifetime = _range / _projectileSpeed;
                Destroy(projectileObj, lifetime);
            }
        }

        /// <summary>
        /// Mermi prefab'ı atanmadıysa basit bir küre oluşturur.
        /// </summary>
        private GameObject CreateAutoProjectilePrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefab.name = "Projectile_Auto";
            prefab.tag = "Projectile";
            prefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            prefab.SetActive(false);

            // Renk ata (sarı mermi)
            Renderer rend = prefab.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
                mat.color = Color.yellow;
                mat.SetColor("_EmissionColor", Color.yellow * 2f);
                rend.material = mat;
            }

            // Collider'ı trigger yap
            SphereCollider col = prefab.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true;

            // Rigidbody ekle
            Rigidbody rb = prefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Projectile bileşeni ekle
            prefab.AddComponent<Projectile>();

            prefab.transform.SetParent(transform);

            return prefab;
        }

        #endregion
    }
}
