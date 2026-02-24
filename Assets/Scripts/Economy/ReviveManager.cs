using UnityEngine;
using System;

/// <summary>
/// Manages the revive flow.
/// - Max 3 revives per level.
/// - Revive = rewarded-ad flow (stub).
/// - Revive restores player HP with 3-second invulnerability shield.
/// - Boss HP is preserved exactly (no regen, no reduction).
/// - 15 % coin penalty applied by CoinManager.
/// - XP is NOT penalised.
///
/// Inspector Setup:
///   - maxRevivesPerLevel:       3
///   - invulnerabilityDuration:  3
///   - reviveUI: drag the ReviveUI component here
///
/// Attach to the GameManager or a dedicated ReviveManager GameObject.
/// </summary>
public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance { get; private set; }

    [Header("Revive Settings")]
    [SerializeField] private int   maxRevivesPerLevel      = 3;
    [SerializeField] private float invulnerabilityDuration = 3f;
    [SerializeField] private ReviveUI reviveUI;

    public int  RevivesUsed { get; private set; }
    public bool CanRevive   => RevivesUsed < maxRevivesPerLevel;

    private float savedBossHP = -1f;
    private HealthComponent playerHealth;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthComponent>();
            playerHealth.OnDeath += OnPlayerDied;
        }
    }

    // ──────────────────────────────────────────────
    // Event handlers
    // ──────────────────────────────────────────────

    void OnPlayerDied()
    {
        SaveBossHP();

        if (CanRevive)
            reviveUI?.Show(RevivesUsed + 1, maxRevivesPerLevel, DoRevive, OnDeclined);
        else
            GameManager.Instance?.EndLevel(false);
    }

    // ──────────────────────────────────────────────
    // Revive flow
    // ──────────────────────────────────────────────

    public void DoRevive()
    {
        RevivesUsed++;
        CoinManager.Instance?.ApplyRevivePenalty();

        if (playerHealth != null)
        {
            playerHealth.RestoreToFull();
            playerHealth.SetInvulnerable(invulnerabilityDuration);
        }

        // Restore boss HP to the exact value it had when player died
        RestoreBossHP();

        GameManager.Instance?.ResumeGame();
    }

    void OnDeclined()
    {
        GameManager.Instance?.EndLevel(false);
    }

    // ──────────────────────────────────────────────
    // Boss HP preservation
    // ──────────────────────────────────────────────

    void SaveBossHP()
    {
        var boss = FindFirstObjectByType<BossEnemy>();
        if (boss != null)
            savedBossHP = boss.GetComponent<HealthComponent>().CurrentHealth;
    }

    void RestoreBossHP()
    {
        if (savedBossHP < 0f) return;
        var boss = FindFirstObjectByType<BossEnemy>();
        boss?.GetComponent<HealthComponent>()?.ForceSetHealth(savedBossHP);
    }

    // ──────────────────────────────────────────────
    // Reset
    // ──────────────────────────────────────────────

    public void ResetForNewLevel()
    {
        RevivesUsed = 0;
        savedBossHP = -1f;
    }
}
