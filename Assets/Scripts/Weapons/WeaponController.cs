using UnityEngine;

/// <summary>
/// Auto-fires projectiles at the current target continuously (no reload).
///
/// Inspector Setup:
///   - projectilePrefab: drag your Projectile prefab here
///   - firePoint: drag an empty child Transform positioned at the weapon muzzle
///   - fireRate: shots per second (default 2)
///   - projectileSpeed: units/second (default 10)
///   - weaponRange: must match TargetSelector.detectionRadius (default 12)
///   - damage: base damage per projectile (default 10)
///
/// Runtime: damageBonus and attackSpeedBonus are set by UpgradeManager.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform  firePoint;

    [Header("Weapon Stats")]
    [SerializeField] private float fireRate       = 2f;   // shots/sec
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float weaponRange    = 12f;
    [SerializeField] private float damage         = 10f;

    private const float MinFireRate = 0.1f; // safety floor to prevent divide-by-zero

    // Applied at runtime by UpgradeManager
    [HideInInspector] public float damageBonus       = 0f;
    [HideInInspector] public float attackSpeedBonus  = 0f; // extra shots/sec

    private TargetSelector targetSelector;
    private float fireTimer;

    void Start()
    {
        targetSelector = GetComponent<TargetSelector>();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryFire();
            fireTimer = 1f / Mathf.Max(MinFireRate, fireRate + attackSpeedBonus);
        }
    }

    void TryFire()
    {
        if (targetSelector == null || targetSelector.CurrentTarget == null) return;

        Transform target = targetSelector.CurrentTarget;
        Vector3 dir = (target.position - firePoint.position);
        dir.y = 0f; // keep projectile flat
        dir.Normalize();

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
        var p = proj.GetComponent<Projectile>();
        if (p != null)
            p.Init(dir, projectileSpeed, weaponRange, damage + damageBonus);
    }
}
