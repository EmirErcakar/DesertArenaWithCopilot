using System.Collections.Generic;
using UnityEngine;
using DesertArena.XP;

namespace DesertArena.Levels
{
    /// <summary>
    /// ScriptableObject defining the configuration for a single arena.
    /// Create via Assets > Create > DesertArena > Arena Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewArenaData", menuName = "DesertArena/Arena Data")]
    public class ArenaData : ScriptableObject
    {
        /// <summary>Zero-based index of this arena.</summary>
        [Header("Arena Identity")]
        [Tooltip("Zero-based index of this arena.")]
        public int arenaIndex;

        /// <summary>Display name of the arena.</summary>
        [Tooltip("Display name of the arena.")]
        public string arenaName = "Desert Arena";

        /// <summary>Total number of levels in this arena.</summary>
        [Header("Level Configuration")]
        [Tooltip("Total number of levels in this arena.")]
        public int totalLevels = 35;

        /// <summary>Enemy prefabs available to spawn in this arena.</summary>
        [Header("Enemies")]
        [Tooltip("Enemy prefabs available to spawn in this arena.")]
        public List<GameObject> availableEnemyPrefabs = new List<GameObject>();

        /// <summary>Weapon feature unlocked when entering this arena.</summary>
        [Header("Unlocks")]
        [Tooltip("Weapon feature unlocked when entering this arena.")]
        public WeaponFeatureType weaponFeatureUnlock = WeaponFeatureType.None;

        /// <summary>Identifier for the background theme (e.g., "sand", "oasis", "ruins").</summary>
        [Header("Visuals")]
        [Tooltip("Identifier for the background theme (e.g., 'sand', 'oasis', 'ruins').")]
        public string backgroundThemeId = "sand";

        /// <summary>Multiplier applied to enemy stats in this arena.</summary>
        [Header("Difficulty")]
        [Tooltip("Multiplier applied to enemy stats in this arena.")]
        public float difficultyMultiplier = 1f;
    }
}
