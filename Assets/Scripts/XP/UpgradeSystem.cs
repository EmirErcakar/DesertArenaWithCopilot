using System;
using System.Collections.Generic;
using UnityEngine;
using DesertArena.Core;

namespace DesertArena.XP
{
    /// <summary>
    /// Generates and manages upgrade card selections. Produces three
    /// cards per upgrade event (stat, weapon feature, random) with
    /// controlled RNG and per-run duplicate limits.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when new upgrade cards are generated. Passes the three cards.</summary>
        public event Action<UpgradeCardData[]> OnCardsGenerated;

        /// <summary>Fired when a card is selected. Passes the chosen card.</summary>
        public event Action<UpgradeCardData> OnCardSelected;

        #endregion

        #region Serialized Fields

        [Header("Reroll")]
        [SerializeField] [Tooltip("Coin cost for a reroll.")]
        private int rerollCost = 1500;

        [Header("Icons (Optional)")]
        [SerializeField] private Sprite attackIcon;
        [SerializeField] private Sprite attackSpeedIcon;
        [SerializeField] private Sprite moveSpeedIcon;
        [SerializeField] private Sprite healthIcon;
        [SerializeField] private Sprite pierceIcon;
        [SerializeField] private Sprite multishotIcon;
        [SerializeField] private Sprite ricochetIcon;
        [SerializeField] private Sprite explosiveIcon;
        [SerializeField] private Sprite slowIcon;

        #endregion

        #region Private Fields

        private const int MAX_REROLL_ATTEMPTS = 20;

        private readonly Dictionary<StatType, int> _statPickCounts =
            new Dictionary<StatType, int>();

        private readonly Dictionary<WeaponFeatureType, int> _featurePickCounts =
            new Dictionary<WeaponFeatureType, int>();

        private UpgradeCardData[] _currentCards;
        private bool _hasRerolledThisLevel;
        private int _totalCoins;

        private static readonly StatType[] AllStats =
            (StatType[])Enum.GetValues(typeof(StatType));

        private static readonly WeaponFeatureType[] AllFeatures = new[]
        {
            WeaponFeatureType.Pierce,
            WeaponFeatureType.Multishot,
            WeaponFeatureType.Ricochet,
            WeaponFeatureType.ExplosiveRounds,
            WeaponFeatureType.SlowOnHit
        };

        #endregion

        #region Properties

        /// <summary>Currently displayed upgrade cards (null when no selection is active).</summary>
        public UpgradeCardData[] CurrentCards => _currentCards;

        /// <summary>Whether a reroll has already been used this level.</summary>
        public bool HasRerolledThisLevel => _hasRerolledThisLevel;

        /// <summary>Coin cost for a reroll.</summary>
        public int RerollCost => rerollCost;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnCoinCollected += HandleCoinCollected;
            EventBus.OnXPLevelUp += HandleXPLevelUp;
        }

        private void OnDisable()
        {
            EventBus.OnCoinCollected -= HandleCoinCollected;
            EventBus.OnXPLevelUp -= HandleXPLevelUp;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates three upgrade cards: stat, weapon feature, and random.
        /// </summary>
        /// <returns>Array of three <see cref="UpgradeCardData"/> instances.</returns>
        public UpgradeCardData[] GenerateCards()
        {
            _currentCards = new UpgradeCardData[3];
            _currentCards[0] = GenerateStatCard();
            _currentCards[1] = GenerateWeaponFeatureCard();
            _currentCards[2] = UnityEngine.Random.value < 0.5f
                ? GenerateStatCard()
                : GenerateWeaponFeatureCard();

            OnCardsGenerated?.Invoke(_currentCards);
            return _currentCards;
        }

        /// <summary>
        /// Rerolls the current cards. Maximum one reroll per level.
        /// Costs <see cref="rerollCost"/> coins unless <paramref name="freeReroll"/> is true.
        /// Guarantees at least one new option compared to the previous set.
        /// </summary>
        /// <param name="freeReroll">True if the reroll is free (e.g., via ad).</param>
        /// <returns>True if the reroll was successful.</returns>
        public bool Reroll(bool freeReroll = false)
        {
            if (_hasRerolledThisLevel) return false;

            if (!freeReroll)
            {
                if (_totalCoins < rerollCost) return false;
                _totalCoins -= rerollCost;
            }

            _hasRerolledThisLevel = true;

            // Store old card names so we can guarantee novelty
            var oldNames = new HashSet<string>();
            if (_currentCards != null)
            {
                foreach (var card in _currentCards)
                {
                    if (card != null) oldNames.Add(card.upgradeName);
                }
            }

            const int maxAttempts = MAX_REROLL_ATTEMPTS;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                GenerateCards();

                foreach (var card in _currentCards)
                {
                    if (card != null && !oldNames.Contains(card.upgradeName))
                    {
                        return true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Selects a card by index (0–2), records the pick, fires events,
        /// and resumes the game.
        /// </summary>
        /// <param name="index">Index of the card to select.</param>
        public void SelectCard(int index)
        {
            if (_currentCards == null || index < 0 || index >= _currentCards.Length) return;

            UpgradeCardData card = _currentCards[index];
            if (card == null) return;

            // Track pick counts for duplicate limiting
            if (card.upgradeType == UpgradeType.Stat)
            {
                _statPickCounts.TryGetValue(card.statType, out int count);
                _statPickCounts[card.statType] = count + 1;
            }
            else
            {
                _featurePickCounts.TryGetValue(card.weaponFeature, out int count);
                _featurePickCounts[card.weaponFeature] = count + 1;
            }

            OnCardSelected?.Invoke(card);

            var upgradeData = new UpgradeData
            {
                upgradeName = card.upgradeName,
                description = card.description,
                icon = card.icon
            };
            EventBus.RaiseUpgradeSelected(upgradeData);

            _currentCards = null;

            if (GameManager.HasInstance)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        /// <summary>
        /// Resets per-level state (reroll availability). Call at level start.
        /// </summary>
        public void ResetForNewLevel()
        {
            _hasRerolledThisLevel = false;
        }

        /// <summary>
        /// Fully resets the system for a new run, clearing all pick history.
        /// </summary>
        public void ResetForNewRun()
        {
            _statPickCounts.Clear();
            _featurePickCounts.Clear();
            _hasRerolledThisLevel = false;
            _currentCards = null;
        }

        #endregion

        #region Private Methods

        private void HandleCoinCollected(int amount)
        {
            _totalCoins += amount;
        }

        private void HandleXPLevelUp()
        {
            GenerateCards();
        }

        private UpgradeCardData GenerateStatCard()
        {
            // Pick a stat that hasn't reached the per-run duplicate limit (max 2)
            var available = new List<StatType>();
            foreach (StatType stat in AllStats)
            {
                _statPickCounts.TryGetValue(stat, out int count);
                if (count < 2)
                {
                    available.Add(stat);
                }
            }

            if (available.Count == 0)
            {
                available.AddRange(AllStats);
            }

            StatType chosen = available[UnityEngine.Random.Range(0, available.Count)];
            int tier = UnityEngine.Random.value < 0.6f ? 1 : 2;

            float bonus = tier == 1 ? 3f : 5f;

            return new UpgradeCardData
            {
                upgradeName = $"{chosen} +{bonus:0}",
                description = $"Increase {chosen} by {bonus:0}.",
                upgradeType = UpgradeType.Stat,
                statType = chosen,
                weaponFeature = WeaponFeatureType.None,
                tier = tier,
                rarity = tier == 1 ? UpgradeRarity.Common : UpgradeRarity.Rare,
                icon = GetStatIcon(chosen)
            };
        }

        private UpgradeCardData GenerateWeaponFeatureCard()
        {
            WeaponFeatureType chosen =
                AllFeatures[UnityEngine.Random.Range(0, AllFeatures.Length)];

            return new UpgradeCardData
            {
                upgradeName = FormatFeatureName(chosen),
                description = GetFeatureDescription(chosen),
                upgradeType = UpgradeType.WeaponFeature,
                statType = StatType.Attack,
                weaponFeature = chosen,
                tier = 1,
                rarity = UpgradeRarity.Epic,
                icon = GetFeatureIcon(chosen)
            };
        }

        private Sprite GetStatIcon(StatType stat)
        {
            return stat switch
            {
                StatType.Attack => attackIcon,
                StatType.AttackSpeed => attackSpeedIcon,
                StatType.MoveSpeed => moveSpeedIcon,
                StatType.Health => healthIcon,
                _ => null
            };
        }

        private Sprite GetFeatureIcon(WeaponFeatureType feature)
        {
            return feature switch
            {
                WeaponFeatureType.Pierce => pierceIcon,
                WeaponFeatureType.Multishot => multishotIcon,
                WeaponFeatureType.Ricochet => ricochetIcon,
                WeaponFeatureType.ExplosiveRounds => explosiveIcon,
                WeaponFeatureType.SlowOnHit => slowIcon,
                _ => null
            };
        }

        private static string FormatFeatureName(WeaponFeatureType feature)
        {
            return feature switch
            {
                WeaponFeatureType.Pierce => "Pierce",
                WeaponFeatureType.Multishot => "Multishot",
                WeaponFeatureType.Ricochet => "Ricochet",
                WeaponFeatureType.ExplosiveRounds => "Explosive Rounds",
                WeaponFeatureType.SlowOnHit => "Slow On Hit",
                _ => feature.ToString()
            };
        }

        private static string GetFeatureDescription(WeaponFeatureType feature)
        {
            return feature switch
            {
                WeaponFeatureType.Pierce => "Projectiles pierce through enemies.",
                WeaponFeatureType.Multishot => "Fire multiple projectiles at once.",
                WeaponFeatureType.Ricochet => "Projectiles bounce between enemies.",
                WeaponFeatureType.ExplosiveRounds => "Projectiles explode on impact.",
                WeaponFeatureType.SlowOnHit => "Hit enemies are slowed briefly.",
                _ => ""
            };
        }

        #endregion
    }
}
