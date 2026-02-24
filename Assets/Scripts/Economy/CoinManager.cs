using UnityEngine;
using System;

/// <summary>
/// Tracks coins — per-session (earned this level) and total (persistent).
///
/// Inspector Setup:
///   - No required fields. Attach to the GameManager GameObject.
///
/// Usage:
///   AddCoins(n)         — called by EnemyBase on death
///   SpendCoins(n)       — returns false if not enough session coins
///   ApplyRevivePenalty()— removes 15 % of session coins on revive
///   CommitToTotal()     — saves session coins to PlayerPrefs at level end
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private const string TotalCoinsKey = "TotalCoins";

    public int TotalCoins   { get; private set; }
    public int SessionCoins { get; private set; }

    /// <summary>Fired with new SessionCoins count whenever it changes.</summary>
    public event Action<int> OnSessionCoinsChanged;

    /// <summary>Fired with new TotalCoins after CommitToTotal.</summary>
    public event Action<int> OnTotalCoinsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance   = this;
        TotalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        SessionCoins += amount;
        OnSessionCoinsChanged?.Invoke(SessionCoins);
    }

    /// <summary>Returns true and spends coins if SessionCoins >= amount.</summary>
    public bool SpendCoins(int amount)
    {
        if (SessionCoins < amount) return false;
        SessionCoins -= amount;
        OnSessionCoinsChanged?.Invoke(SessionCoins);
        return true;
    }

    /// <summary>Deducts 15 % of session coins as a revive penalty.</summary>
    public void ApplyRevivePenalty()
    {
        int penalty  = Mathf.RoundToInt(SessionCoins * 0.15f);
        SessionCoins = Mathf.Max(0, SessionCoins - penalty);
        OnSessionCoinsChanged?.Invoke(SessionCoins);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[CoinManager] Revive penalty: -{penalty} coins. Session total: {SessionCoins}");
#endif
    }

    /// <summary>Saves session coins to the persistent total at level end.</summary>
    public void CommitToTotal()
    {
        TotalCoins += SessionCoins;
        PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins);
        PlayerPrefs.Save();
        OnTotalCoinsChanged?.Invoke(TotalCoins);
    }

    /// <summary>Reset session coins (e.g., when returning to main menu).</summary>
    public void ResetSession()
    {
        SessionCoins = 0;
        OnSessionCoinsChanged?.Invoke(0);
    }
}
