using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertArena.XP;

namespace DesertArena.UI
{
    /// <summary>
    /// Displays the three upgrade cards when the XP bar fills.
    /// Pauses the game, darkens the background, and shows cards
    /// with a flip animation. Supports one reroll per level.
    /// </summary>
    public class UpgradeCardUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [SerializeField] [Tooltip("Root panel containing the upgrade card display.")]
        private GameObject panelRoot;

        [SerializeField] [Tooltip("Dark overlay background image.")]
        private Image darkOverlay;

        [Header("Cards")]
        [SerializeField] [Tooltip("Card root GameObjects (expects 3 elements).")]
        private GameObject[] cardRoots = new GameObject[3];

        [SerializeField] [Tooltip("Card icon images (expects 3 elements).")]
        private Image[] cardIcons = new Image[3];

        [SerializeField] [Tooltip("Card label texts (expects 3 elements).")]
        private TextMeshProUGUI[] cardLabels = new TextMeshProUGUI[3];

        [SerializeField] [Tooltip("Card description texts (expects 3 elements).")]
        private TextMeshProUGUI[] cardDescriptions = new TextMeshProUGUI[3];

        [SerializeField] [Tooltip("Card frame images for tier coloring (expects 3 elements).")]
        private Image[] cardFrames = new Image[3];

        [SerializeField] [Tooltip("Card select buttons (expects 3 elements).")]
        private Button[] cardButtons = new Button[3];

        [Header("Reroll")]
        [SerializeField] [Tooltip("Reroll button.")]
        private Button rerollButton;

        [SerializeField] [Tooltip("Reroll cost label.")]
        private TextMeshProUGUI rerollCostText;

        [Header("Frame Colors")]
        [SerializeField] [Tooltip("Frame color for tier-1 stat cards (blue).")]
        private Color statTier1Color = new Color(0.3f, 0.5f, 1f);

        [SerializeField] [Tooltip("Frame color for tier-2 stat cards (purple).")]
        private Color statTier2Color = new Color(0.6f, 0.3f, 0.9f);

        [SerializeField] [Tooltip("Frame color for weapon feature cards (red).")]
        private Color weaponFeatureColor = new Color(0.9f, 0.2f, 0.2f);

        [SerializeField] [Tooltip("Color of the dark overlay.")]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.7f);

        [Header("Animation")]
        [SerializeField] [Tooltip("Duration of the card flip animation in seconds.")]
        private float flipDuration = 0.3f;

        [Header("References")]
        [SerializeField] [Tooltip("UpgradeSystem component in the scene.")]
        private UpgradeSystem upgradeSystem;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.OnCardsGenerated += ShowCards;
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(OnRerollClicked);
            }

            for (int i = 0; i < cardButtons.Length; i++)
            {
                if (cardButtons[i] != null)
                {
                    int index = i;
                    cardButtons[i].onClick.AddListener(() => OnCardClicked(index));
                }
            }
        }

        private void OnDisable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.OnCardsGenerated -= ShowCards;
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(OnRerollClicked);
            }

            for (int i = 0; i < cardButtons.Length; i++)
            {
                if (cardButtons[i] != null)
                {
                    cardButtons[i].onClick.RemoveAllListeners();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays upgrade cards with a staggered flip animation.
        /// </summary>
        /// <param name="cards">The three upgrade cards to display.</param>
        public void ShowCards(UpgradeCardData[] cards)
        {
            if (cards == null || cards.Length < 3) return;

            if (panelRoot != null) panelRoot.SetActive(true);

            if (darkOverlay != null)
            {
                darkOverlay.color = overlayColor;
            }

            UpdateRerollButton();

            for (int i = 0; i < 3; i++)
            {
                PopulateCard(i, cards[i]);
                if (cardRoots[i] != null)
                {
                    StartCoroutine(FlipCardAnimation(cardRoots[i].transform, i * 0.15f));
                }
            }
        }

        /// <summary>
        /// Hides the upgrade card panel.
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void PopulateCard(int index, UpgradeCardData card)
        {
            if (card == null) return;

            if (index < cardIcons.Length && cardIcons[index] != null)
            {
                cardIcons[index].sprite = card.icon;
                cardIcons[index].enabled = card.icon != null;
            }

            if (index < cardLabels.Length && cardLabels[index] != null)
            {
                cardLabels[index].text = card.upgradeName;
            }

            if (index < cardDescriptions.Length && cardDescriptions[index] != null)
            {
                cardDescriptions[index].text = card.description;
            }

            if (index < cardFrames.Length && cardFrames[index] != null)
            {
                cardFrames[index].color = GetFrameColor(card);
            }
        }

        private Color GetFrameColor(UpgradeCardData card)
        {
            if (card.upgradeType == UpgradeType.WeaponFeature)
            {
                return weaponFeatureColor;
            }
            return card.tier >= 2 ? statTier2Color : statTier1Color;
        }

        /// <summary>
        /// Simple flip animation: scales X from 0 to 1 to simulate a card reveal.
        /// Uses unscaled time so it works while the game is paused.
        /// </summary>
        private IEnumerator FlipCardAnimation(Transform cardTransform, float delay)
        {
            cardTransform.localScale = new Vector3(0f, 1f, 1f);

            float waited = 0f;
            while (waited < delay)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            float elapsed = 0f;
            while (elapsed < flipDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / flipDuration);
                float scaleX = Mathf.Sin(t * Mathf.PI * 0.5f);
                cardTransform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }

            cardTransform.localScale = Vector3.one;
        }

        private void OnCardClicked(int index)
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.SelectCard(index);
            }
            Hide();
        }

        private void OnRerollClicked()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.Reroll();
            }
            UpdateRerollButton();
        }

        private void UpdateRerollButton()
        {
            if (rerollButton == null) return;

            bool canReroll = upgradeSystem != null && !upgradeSystem.HasRerolledThisLevel;
            rerollButton.interactable = canReroll;

            if (rerollCostText != null && upgradeSystem != null)
            {
                rerollCostText.text = canReroll
                    ? $"Reroll ({upgradeSystem.RerollCost} coins)"
                    : "Rerolled";
            }
        }

        #endregion
    }
}
