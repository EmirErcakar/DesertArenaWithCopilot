using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Upgrade selection overlay. Shown whenever XPSystem triggers an upgrade.
/// Pauses the game (Time.timeScale = 0) while open.
///
/// Inspector Setup:
///   - panel:               root panel GameObject (dark semi-transparent background)
///   - cardSlots:           3 UpgradeCard references
///   - rerollButton:        the Reroll button
///   - rerollInfoText:      shows reroll cost / availability
///   - rerollCoinCost:      1500 (coins) or watch a rewarded ad
///
/// Scene hierarchy example:
///   UpgradeScreen (Canvas overlay)
///     ├─ DarkBackground (Image alpha ~0.7)
///     ├─ CardsContainer
///     │    ├─ Card1 (UpgradeCard)
///     │    ├─ Card2 (UpgradeCard)
///     │    └─ Card3 (UpgradeCard)
///     └─ RerollButton
/// </summary>
public class UpgradeScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject  panel;
    [SerializeField] private UpgradeCard[] cardSlots;       // expects exactly 3
    [SerializeField] private Button      rerollButton;
    [SerializeField] private TMP_Text    rerollInfoText;

    [Header("Reroll Settings")]
    [SerializeField] private int rerollCoinCost = 1500;

    private List<UpgradeDefinition> currentChoices;

    // ──────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────

    void Awake()
    {
        panel.SetActive(false);
        rerollButton.onClick.AddListener(OnRerollClicked);
    }

    void Start()
    {
        if (XPSystem.Instance != null)
            XPSystem.Instance.OnUpgradeTriggered += ShowUpgradeScreen;
    }

    void OnDestroy()
    {
        if (XPSystem.Instance != null)
            XPSystem.Instance.OnUpgradeTriggered -= ShowUpgradeScreen;
    }

    // ──────────────────────────────────────────────
    // Show / Hide
    // ──────────────────────────────────────────────

    void ShowUpgradeScreen()
    {
        currentChoices = UpgradeManager.Instance.GetUpgradeChoices();
        DisplayCards(currentChoices);
        UpdateRerollButton();
        panel.SetActive(true);
    }

    void DisplayCards(List<UpgradeDefinition> choices)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < choices.Count && choices[i] != null)
            {
                cardSlots[i].gameObject.SetActive(true);
                // Stagger each card's flip: 0 s, 0.15 s, 0.30 s
                cardSlots[i].Setup(choices[i], OnCardSelected, i * 0.15f);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    void UpdateRerollButton()
    {
        bool canReroll = UpgradeManager.Instance != null && UpgradeManager.Instance.CanReroll();
        rerollButton.interactable = canReroll;

        if (rerollInfoText != null)
        {
            rerollInfoText.text = canReroll
                ? $"Reroll — {rerollCoinCost} coins or watch ad (1 left)"
                : "No rerolls left";
        }
    }

    // ──────────────────────────────────────────────
    // Callbacks
    // ──────────────────────────────────────────────

    void OnCardSelected(UpgradeDefinition upgrade)
    {
        UpgradeManager.Instance?.ApplyUpgrade(upgrade);
        panel.SetActive(false);
        GameManager.Instance?.ResumeGame();
    }

    void OnRerollClicked()
    {
        if (UpgradeManager.Instance == null || !UpgradeManager.Instance.CanReroll()) return;

        if (CoinManager.Instance != null && CoinManager.Instance.SessionCoins >= rerollCoinCost)
        {
            // Spend coins
            CoinManager.Instance.SpendCoins(rerollCoinCost);
            DoReroll();
        }
        else
        {
            // Watch a rewarded ad instead
            AdServiceStub.Instance?.ShowRewardedAd(DoReroll);
        }
    }

    void DoReroll()
    {
        currentChoices = UpgradeManager.Instance.Reroll(currentChoices);
        DisplayCards(currentChoices);
        UpdateRerollButton();
    }
}
