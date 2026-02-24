using System;
using UnityEngine;

namespace DesertArena.XP
{
    /// <summary>
    /// Types of upgrades available to the player.
    /// </summary>
    public enum UpgradeType
    {
        Stat,
        WeaponFeature
    }

    /// <summary>
    /// Player stat categories that can be upgraded.
    /// </summary>
    public enum StatType
    {
        Attack,
        AttackSpeed,
        MoveSpeed,
        Health
    }

    /// <summary>
    /// Weapon feature abilities that can be unlocked or enhanced.
    /// </summary>
    public enum WeaponFeatureType
    {
        None,
        Pierce,
        Multishot,
        Ricochet,
        ExplosiveRounds,
        SlowOnHit
    }

    /// <summary>
    /// Rarity tiers for upgrade cards.
    /// </summary>
    public enum UpgradeRarity
    {
        Common,
        Rare,
        Epic
    }

    /// <summary>
    /// Data container for a single upgrade card. Not a MonoBehaviour;
    /// created at runtime by the <see cref="UpgradeSystem"/>.
    /// </summary>
    [Serializable]
    public class UpgradeCardData
    {
        /// <summary>Display name of the upgrade.</summary>
        public string upgradeName;

        /// <summary>Short description shown on the card.</summary>
        public string description;

        /// <summary>Whether this is a stat boost or weapon feature.</summary>
        public UpgradeType upgradeType;

        /// <summary>Which stat is affected (only relevant when upgradeType is Stat).</summary>
        public StatType statType;

        /// <summary>Which weapon feature is granted (only relevant when upgradeType is WeaponFeature).</summary>
        public WeaponFeatureType weaponFeature;

        /// <summary>Tier of the upgrade. 1 = small boost (+3), 2 = large boost (+5).</summary>
        public int tier;

        /// <summary>Rarity of the upgrade card.</summary>
        public UpgradeRarity rarity;

        /// <summary>Icon displayed on the upgrade card (assigned at runtime).</summary>
        public Sprite icon;
    }
}
