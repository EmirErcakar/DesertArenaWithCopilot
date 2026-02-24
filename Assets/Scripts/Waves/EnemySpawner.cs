using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemies periodically from a set of spawn points.
///
/// Inspector Setup:
///   - enemyPrefab:     drag the enemy prefab to spawn
///   - spawnPoints:     array of Transform positions (set around arena edges)
///   - spawnInterval:   seconds between spawn attempts (default 3)
///   - maxAlive:        max simultaneous enemies from this spawner (default 5)
///
/// StartSpawning() / StopSpawning() are called by WaveManager.
/// SpawnOne() is called by WaveManager for the boss.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int   maxAlive      = 5;

    public bool IsSpawning { get; private set; }

    private Coroutine spawnCoroutine;
    private int currentAlive;

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void StartSpawning()
    {
        if (IsSpawning) return;
        IsSpawning     = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        IsSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>Spawn a single enemy immediately (used for boss).</summary>
    public void SpawnOne()
    {
        SpawnEnemy();
    }

    // ──────────────────────────────────────────────
    // Private
    // ──────────────────────────────────────────────

    IEnumerator SpawnLoop()
    {
        while (IsSpawning)
        {
            if (currentAlive < maxAlive)
                SpawnEnemy();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp  = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject go = Instantiate(enemyPrefab, sp.position, Quaternion.identity);
        currentAlive++;

        // Track when this enemy dies
        var health = go.GetComponent<HealthComponent>();
        if (health != null)
            health.OnDeath += () => currentAlive = Mathf.Max(0, currentAlive - 1);
    }
}
