using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Revive prompt UI. Shown when the player dies and has revives remaining.
///
/// Inspector Setup:
///   - panel:          root panel (covers screen, semi-transparent dark background)
///   - reviveButton:   "Revive (Watch Ad)" button
///   - declineButton:  "Quit" / "No Thanks" button
///   - countText:      e.g. "Revive 1/3"
///   - penaltyText:    e.g. "–15% coins"
///
/// Attach to HUD Canvas.
/// </summary>
public class ReviveUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     reviveButton;
    [SerializeField] private Button     declineButton;
    [SerializeField] private TMP_Text   countText;
    [SerializeField] private TMP_Text   penaltyText;

    private Action onReviveAccepted;
    private Action onReviveDeclined;

    void Awake()
    {
        panel.SetActive(false);
        reviveButton.onClick.AddListener(OnReviveClicked);
        declineButton.onClick.AddListener(OnDeclineClicked);
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void Show(int currentRevive, int maxRevives, Action onAccept, Action onDecline)
    {
        onReviveAccepted = onAccept;
        onReviveDeclined = onDecline;

        if (countText   != null) countText.text   = $"Revive {currentRevive}/{maxRevives}";
        if (penaltyText != null) penaltyText.text  = "Penalty: –15% coins";

        panel.SetActive(true);
        Time.timeScale = 0f; // pause while prompt is visible
    }

    // ──────────────────────────────────────────────
    // Button handlers
    // ──────────────────────────────────────────────

    void OnReviveClicked()
    {
        reviveButton.interactable  = false;
        declineButton.interactable = false;

        AdServiceStub.Instance?.ShowRewardedAd(() =>
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
            onReviveAccepted?.Invoke();
        });
    }

    void OnDeclineClicked()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        onReviveDeclined?.Invoke();
    }
}
