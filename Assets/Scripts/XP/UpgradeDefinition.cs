using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single upgrade option shown on the upgrade cards.
///
/// How to create:
///   Right-click in Project window > Create > DesertArena > Upgrade Definition
///
/// Tiers:
///   Blue   (+3 level) — minor stat boosts
///   Purple (+5 level) — stronger stat boosts
///   Red    (rare)     — primarily weapon features
/// </summary>
[CreateAssetMenu(fileName = "Upgrade_New", menuName = "DesertArena/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public enum UpgradeType { Stat, WeaponFeature }
    public enum UpgradeTier { Blue = 3, Purple = 5, Red = 7 }
    public enum StatType    { None, Attack, AttackSpeed, MoveSpeed, Health }

    [Header("Card Text")]
    public string upgradeName;
    [TextArea(1, 3)]
    public string description;
    public Sprite icon;

    [Header("Classification")]
    public UpgradeType upgradeType = UpgradeType.Stat;
    public UpgradeTier tier        = UpgradeTier.Blue;
    public StatType    statType    = StatType.None;

    [Header("Numeric Value")]
    [Tooltip("Stat boost amount (e.g. 3 for +3 Attack). Feature upgrades can use 1.")]
    public float value = 3f;

    [Header("Duplicate Rules")]
    [Tooltip("Maximum times this upgrade can be offered/taken per run.")]
    public int maxTimesPerRun = 2;

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    public Color GetFrameColor()
    {
        switch (tier)
        {
            case UpgradeTier.Blue:   return new Color(0.20f, 0.45f, 1.00f);
            case UpgradeTier.Purple: return new Color(0.60f, 0.10f, 0.90f);
            case UpgradeTier.Red:    return new Color(1.00f, 0.10f, 0.10f);
            default:                 return Color.white;
        }
    }
}
