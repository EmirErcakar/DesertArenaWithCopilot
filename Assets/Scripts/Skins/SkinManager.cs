using System;
using UnityEngine;
using DesertArena.Utilities;

namespace DesertArena.Skins
{
    /// <summary>
    /// Singleton that manages all available skins, unlock states, and the
    /// currently equipped skin. All skins are visible in the shop but only
    /// unlocked skins can be equipped. State is saved in PlayerPrefs.
    /// </summary>
    public class SkinManager : Singleton<SkinManager>
    {
        #region Constants

        private const string PREFS_EQUIPPED = "Skin_Equipped";
        private const string PREFS_UNLOCKED_PREFIX = "Skin_Unlocked_";

        #endregion

        #region Serialized Fields

        [Header("Skins")]
        [SerializeField] [Tooltip("All skins available in the game. Assign via inspector.")]
        private SkinData[] _allSkins;

        #endregion

        #region Private Fields

        private string _equippedSkinId;

        #endregion

        #region Events

        /// <summary>Fired when a skin is unlocked. Passes the skin ID.</summary>
        public event Action<string> OnSkinUnlocked;

        /// <summary>Fired when the equipped skin changes. Passes the new skin ID.</summary>
        public event Action<string> OnSkinEquipped;

        #endregion

        #region Properties

        /// <summary>All registered skins.</summary>
        public SkinData[] AllSkins => _allSkins;

        #endregion

        #region Unity Lifecycle

        protected override void OnSingletonAwake()
        {
            LoadSkins();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads unlock and equip state from PlayerPrefs.
        /// </summary>
        public void LoadSkins()
        {
            if (_allSkins == null) return;

            _equippedSkinId = PlayerPrefs.GetString(PREFS_EQUIPPED, string.Empty);
        }

        /// <summary>
        /// Returns true if the skin with the given ID is unlocked.
        /// </summary>
        public bool IsSkinUnlocked(string skinId)
        {
            return PlayerPrefs.GetInt(PREFS_UNLOCKED_PREFIX + skinId, 0) == 1;
        }

        /// <summary>
        /// Unlocks a skin by ID. Saves the state immediately.
        /// </summary>
        /// <param name="skinId">The unique skin identifier.</param>
        public void UnlockSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return;
            if (IsSkinUnlocked(skinId)) return;

            PlayerPrefs.SetInt(PREFS_UNLOCKED_PREFIX + skinId, 1);
            PlayerPrefs.Save();

            OnSkinUnlocked?.Invoke(skinId);
        }

        /// <summary>
        /// Equips a skin if it is unlocked.
        /// </summary>
        /// <param name="skinId">The unique skin identifier.</param>
        /// <returns>True if the skin was equipped successfully.</returns>
        public bool EquipSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return false;
            if (!IsSkinUnlocked(skinId)) return false;

            _equippedSkinId = skinId;
            PlayerPrefs.SetString(PREFS_EQUIPPED, _equippedSkinId);
            PlayerPrefs.Save();

            OnSkinEquipped?.Invoke(_equippedSkinId);
            return true;
        }

        /// <summary>
        /// Returns the currently equipped <see cref="SkinData"/>, or null.
        /// </summary>
        public SkinData GetEquippedSkin()
        {
            return FindSkin(_equippedSkinId);
        }

        /// <summary>
        /// Returns the stat boost from the currently equipped skin for a
        /// given stat type.
        /// </summary>
        /// <param name="statType">The stat to query.</param>
        /// <returns>Bonus multiplier (e.g. 0.03 for +3 %).</returns>
        public float GetSkinBoost(SkinStatType statType)
        {
            SkinData equipped = GetEquippedSkin();
            if (equipped == null) return 0f;
            return equipped.GetBoost(statType);
        }

        /// <summary>
        /// Finds a skin by its unique ID.
        /// </summary>
        /// <param name="skinId">The skin identifier to search for.</param>
        /// <returns>The matching <see cref="SkinData"/> or null.</returns>
        public SkinData FindSkin(string skinId)
        {
            if (_allSkins == null || string.IsNullOrEmpty(skinId)) return null;

            for (int i = 0; i < _allSkins.Length; i++)
            {
                if (_allSkins[i] != null && _allSkins[i].SkinId == skinId)
                    return _allSkins[i];
            }

            return null;
        }

        #endregion
    }
}
