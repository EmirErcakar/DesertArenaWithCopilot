using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls wave spawning timing for Arena 1 Level 1.
///
/// Wave plan (120 s total):
///   Phase 0 [  0 – 35 s] : MeleeEnemy (knife) only
///   Phase 1 [ 35 – 65 s] : MeleeEnemy + SwordEnemy
///   Phase 2 [ 65 – end ] : MeleeEnemy + SwordEnemy + RangedEnemy
///   Boss level: after 120 s a 10-second countdown plays, then boss spawns.
///
/// Inspector Setup:
///   - wavePhases: configure 3 phases. For each phase set startTime, endTime, and assign spawners.
///   - bossSpawner: assign boss spawner for levels that are boss levels (every 5 levels).
///   - isBossLevel: tick for boss levels.
///   - levelDuration: 120 (seconds).
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [System.Serializable]
    public class WavePhase
    {
        public string         phaseName;
        public float          startTime;
        [Tooltip("0 = runs to end of level")]
        public float          endTime;
        public EnemySpawner[] spawners;
    }

    [Header("Wave Setup")]
    [SerializeField] private WavePhase[]   wavePhases;
    [SerializeField] private EnemySpawner  bossSpawner;
    [SerializeField] private bool          isBossLevel;
    [SerializeField] private float         levelDuration = 120f;

    private float levelTimer;
    private bool  bossSpawned;
    private bool  levelEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(LevelTimerRoutine());
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        levelTimer += Time.deltaTime;
        TickPhases();
    }

    void TickPhases()
    {
        if (wavePhases == null) return;
        foreach (var phase in wavePhases)
        {
            bool shouldBeActive = levelTimer >= phase.startTime
                && (phase.endTime <= 0f || levelTimer < phase.endTime);

            if (phase.spawners == null) continue;
            foreach (var s in phase.spawners)
            {
                if (s == null) continue;
                if (shouldBeActive && !s.IsSpawning)  s.StartSpawning();
                if (!shouldBeActive && s.IsSpawning)  s.StopSpawning();
            }
        }
    }

    IEnumerator LevelTimerRoutine()
    {
        yield return new WaitForSeconds(levelDuration);

        if (levelEnded) yield break;

        // Stop all spawners
        if (wavePhases != null)
            foreach (var phase in wavePhases)
                if (phase.spawners != null)
                    foreach (var s in phase.spawners)
                        s?.StopSpawning();

        if (isBossLevel)
            StartCoroutine(BossCountdownRoutine());
        else
            GameManager.Instance?.EndLevel(true);
    }

    IEnumerator BossCountdownRoutine()
    {
        BossCountdownUI.Instance?.Show();
        for (int i = 10; i > 0; i--)
        {
            BossCountdownUI.Instance?.UpdateCount(i);
            yield return new WaitForSeconds(1f);
        }
        BossCountdownUI.Instance?.Hide();

        bossSpawned = true;
        bossSpawner?.SpawnOne();
    }

    // ──────────────────────────────────────────────
    // Called by EnemyBase on death
    // ──────────────────────────────────────────────

    public void OnEnemyDied(EnemyBase enemy)
    {
        if (enemy is BossEnemy)
            levelEnded = true;
    }
}
