using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertArena.Core;
using DesertArena.Enemies;
using DesertArena.Player;

namespace DesertArena.UI
{
    /// <summary>
    /// Manages the in-game HUD: player HP bar, XP bar, coin counter,
    /// arena/level indicator, and boss HP bar.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Player HP")]
        [SerializeField] [Tooltip("Image (Filled) for the player HP bar.")]
        private Image hpBarFill;

        [Header("XP Bar")]
        [SerializeField] [Tooltip("Image (Filled, yellow) for the XP bar.")]
        private Image xpBarFill;

        [Header("Coins")]
        [SerializeField] [Tooltip("TextMeshPro text displaying the coin count.")]
        private TextMeshProUGUI coinText;

        [SerializeField] [Tooltip("Prefab for floating '+N' coin text.")]
        private GameObject floatingCoinTextPrefab;

        [SerializeField] [Tooltip("Parent transform for floating text instances.")]
        private Transform floatingTextParent;

        [Header("Level Info")]
        [SerializeField] [Tooltip("TextMeshPro text showing current arena and level.")]
        private TextMeshProUGUI levelIndicatorText;

        [Header("Boss HP")]
        [SerializeField] [Tooltip("Root object of the boss HP bar (hidden until boss spawns).")]
        private GameObject bossHPBarRoot;

        [SerializeField] [Tooltip("Image (Filled) for the boss HP bar.")]
        private Image bossHPBarFill;

        [Header("References")]
        [SerializeField] [Tooltip("PlayerStats component in the scene.")]
        private PlayerStats playerStats;

        [SerializeField] [Tooltip("XPSystem component in the scene.")]
        private XP.XPSystem xpSystem;

        #endregion

        #region Private Fields

        private int _totalCoins;
        private EnemyBase _currentBoss;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnCoinCollected += HandleCoinCollected;
            EventBus.OnBossSpawned += HandleBossSpawned;
            EventBus.OnBossDied += HandleBossDied;

            if (playerStats != null)
            {
                playerStats.OnHealthChanged += UpdateHPBar;
            }
        }

        private void OnDisable()
        {
            EventBus.OnCoinCollected -= HandleCoinCollected;
            EventBus.OnBossSpawned -= HandleBossSpawned;
            EventBus.OnBossDied -= HandleBossDied;

            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHPBar;
            }
        }

        private void Start()
        {
            if (bossHPBarRoot != null) bossHPBarRoot.SetActive(false);

            UpdateCoinDisplay();
            UpdateLevelIndicator();

            if (playerStats != null)
            {
                UpdateHPBar(playerStats.CurrentHP, playerStats.MaxHP);
            }
        }

        private void Update()
        {
            UpdateXPBar();
            UpdateBossHPBar();
        }

        #endregion

        #region Private Methods

        private void UpdateHPBar(float current, float max)
        {
            if (hpBarFill != null)
            {
                hpBarFill.fillAmount = max > 0f ? current / max : 0f;
            }
        }

        private void UpdateXPBar()
        {
            if (xpBarFill != null && xpSystem != null)
            {
                xpBarFill.fillAmount = xpSystem.FillAmount;
            }
        }

        private void HandleCoinCollected(int amount)
        {
            _totalCoins += amount;
            UpdateCoinDisplay();
            SpawnFloatingCoinText(amount);
        }

        private void UpdateCoinDisplay()
        {
            if (coinText != null)
            {
                coinText.text = _totalCoins.ToString();
            }
        }

        /// <summary>
        /// Spawns a floating "+N" text that drifts upward and fades out.
        /// </summary>
        private void SpawnFloatingCoinText(int amount)
        {
            if (floatingCoinTextPrefab == null || floatingTextParent == null) return;

            GameObject obj = Instantiate(floatingCoinTextPrefab, floatingTextParent);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = $"+{amount}";
            }

            StartCoroutine(AnimateFloatingText(obj));
        }

        private IEnumerator AnimateFloatingText(GameObject obj)
        {
            if (obj == null) yield break;

            RectTransform rt = obj.GetComponent<RectTransform>();
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();

            Vector3 startPos = rt.anchoredPosition;
            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = startPos + Vector3.up * (60f * t);
                cg.alpha = 1f - t;
                yield return null;
            }

            Destroy(obj);
        }

        private void UpdateLevelIndicator()
        {
            if (levelIndicatorText == null) return;

            int arena = GameManager.HasInstance
                ? GameManager.Instance.CurrentArena + 1 : 1;
            int level = GameManager.HasInstance
                ? GameManager.Instance.CurrentLevel + 1 : 1;

            levelIndicatorText.text = $"Arena {arena} - Level {level}";
        }

        private void HandleBossSpawned()
        {
            if (bossHPBarRoot != null) bossHPBarRoot.SetActive(true);

            // Locate the boss in the scene for HP tracking
            var boss = FindAnyObjectByType<BossEnemy>();
            if (boss != null)
            {
                _currentBoss = boss;
            }
        }

        private void HandleBossDied()
        {
            _currentBoss = null;
            if (bossHPBarRoot != null) bossHPBarRoot.SetActive(false);
        }

        private void UpdateBossHPBar()
        {
            if (_currentBoss == null || bossHPBarFill == null) return;

            bossHPBarFill.fillAmount = _currentBoss.MaxHP > 0f
                ? _currentBoss.CurrentHP / _currentBoss.MaxHP
                : 0f;
        }

        #endregion
    }
}
