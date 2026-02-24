using UnityEngine;
using System;

/// <summary>
/// Tracks HP for any unit (player, enemies, boss).
/// Fires events on health change and on death.
///
/// Inspector Setup:
///   - maxHealth: starting max HP (default 100)
///   - Attach to any unit that can be damaged
/// </summary>
public class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    public float MaxHealth    => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool  IsDead        { get; private set; }

    /// <summary>Fired with (currentHP, maxHP) whenever health changes.</summary>
    public event Action<float, float> OnHealthChanged;

    /// <summary>Fired once when CurrentHealth reaches 0.</summary>
    public event Action OnDeath;

    private float invulnerabilityTimer;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead || invulnerabilityTimer > 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Temporarily makes the unit immune to damage (used after revive).</summary>
    public void SetInvulnerable(float duration)
    {
        invulnerabilityTimer = duration;
    }

    /// <summary>Increase/decrease max HP. refill = true restores HP to new max.</summary>
    public void SetMaxHealth(float newMax, bool refill = false)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (refill) CurrentHealth = maxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Full heal and revive the unit (used by ReviveManager).</summary>
    public void RestoreToFull()
    {
        IsDead        = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// Directly sets health to a specific value without triggering death.
    /// Used by ReviveManager to preserve boss HP across revives.
    /// </summary>
    public void ForceSetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    // ──────────────────────────────────────────────
    // Private
    // ──────────────────────────────────────────────

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDeath?.Invoke();
    }
}
