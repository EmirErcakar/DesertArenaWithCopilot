using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesertArena.Skins
{
    /// <summary>
    /// How a skin can be purchased.
    /// </summary>
    public enum SkinPurchaseType
    {
        Coins,
        Fragments,
        RealMoney
    }

    /// <summary>
    /// Stat types a skin can boost.
    /// </summary>
    public enum SkinStatType
    {
        Attack,
        AttackSpeed,
        MoveSpeed,
        Health
    }

    /// <summary>
    /// Serializable key-value pair for skin stat boosts.
    /// </summary>
    [Serializable]
    public struct SkinBoostEntry
    {
        /// <summary>The stat this boost applies to.</summary>
        public SkinStatType StatType;

        /// <summary>Bonus multiplier (e.g., 0.03 = +3 %).</summary>
        [Range(0f, 0.10f)]
        public float BonusPercent;
    }

    /// <summary>
    /// ScriptableObject that defines a single player skin.
    /// Each skin has visual references, purchase requirements, and
    /// optional stat boosts capped at a combined 8-10 %.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkin", menuName = "DesertArena/Skin Data")]
    public class SkinData : ScriptableObject
    {
        #region Serialized Fields

        [Header("Identity")]
        [SerializeField] [Tooltip("Unique identifier for this skin.")]
        private string _skinId;

        [SerializeField] [Tooltip("Display name shown in the shop and inventory.")]
        private string _skinName;

        [SerializeField] [TextArea(2, 4)]
        [Tooltip("Short description of the skin.")]
        private string _description;

        [Header("Visuals (placeholders)")]
        [SerializeField] [Tooltip("Mesh to apply to the player model.")]
        private Mesh _skinMesh;

        [SerializeField] [Tooltip("Material to apply to the player model.")]
        private Material _skinMaterial;

        [SerializeField] [Tooltip("Thumbnail icon for the UI.")]
        private Sprite _icon;

        [Header("Purchase")]
        [SerializeField] [Tooltip("How this skin is acquired.")]
        private SkinPurchaseType _purchaseType = SkinPurchaseType.Coins;

        [SerializeField] [Tooltip("Coin price (if purchaseType is Coins).")]
        private int _price;

        [SerializeField] [Tooltip("Fragments needed (if purchaseType is Fragments).")]
        private int _fragmentsRequired;

        [Header("Stat Boosts")]
        [SerializeField] [Tooltip("Stat boosts provided by this skin. Combined cap: 8-10 %.")]
        private SkinBoostEntry[] _boosts;

        #endregion

        #region Properties

        /// <summary>Unique identifier for this skin.</summary>
        public string SkinId => _skinId;

        /// <summary>Display name.</summary>
        public string SkinName => _skinName;

        /// <summary>Short description.</summary>
        public string Description => _description;

        /// <summary>Mesh placeholder.</summary>
        public Mesh SkinMesh => _skinMesh;

        /// <summary>Material placeholder.</summary>
        public Material SkinMaterial => _skinMaterial;

        /// <summary>Thumbnail icon.</summary>
        public Sprite Icon => _icon;

        /// <summary>How this skin is purchased.</summary>
        public SkinPurchaseType PurchaseType => _purchaseType;

        /// <summary>Coin price.</summary>
        public int Price => _price;

        /// <summary>Fragment cost.</summary>
        public int FragmentsRequired => _fragmentsRequired;

        /// <summary>Array of stat boosts.</summary>
        public SkinBoostEntry[] Boosts => _boosts;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the bonus percentage for a given stat type, or 0 if this
        /// skin does not boost that stat.
        /// </summary>
        /// <param name="statType">The stat to query.</param>
        /// <returns>Bonus multiplier (e.g. 0.03 for +3 %).</returns>
        public float GetBoost(SkinStatType statType)
        {
            if (_boosts == null) return 0f;

            for (int i = 0; i < _boosts.Length; i++)
            {
                if (_boosts[i].StatType == statType)
                    return _boosts[i].BonusPercent;
            }

            return 0f;
        }

        /// <summary>
        /// Returns the total combined boost percentage across all stats.
        /// Used to validate the 8-10 % cap.
        /// </summary>
        public float GetTotalBoostPercent()
        {
            if (_boosts == null) return 0f;

            float total = 0f;
            for (int i = 0; i < _boosts.Length; i++)
            {
                total += _boosts[i].BonusPercent;
            }
            return total;
        }

        #endregion
    }
}
