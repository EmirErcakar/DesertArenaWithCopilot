using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Chest opening UI shown after boss death.
/// Always drops — even if boss was killed after revives.
///
/// Inspector Setup:
///   - panel:           root panel (covers screen)
///   - chestAnimator:   Animator on the chest graphic (trigger "Open")
///   - rewardText:      shows the reward after opening
///   - openButton:      "Open!" button
///   - openX2Button:    "Open x2 (Watch Ad)" button
///   - closeButton:     "Continue" button (shown after reward is revealed)
///
/// BossEnemy calls ChestUI.Instance.ShowChest() on death.
/// After the player closes the chest UI, the level-complete flow runs.
/// </summary>
public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Animator   chestAnimator;
    [SerializeField] private TMP_Text   rewardText;
    [SerializeField] private Button     openButton;
    [SerializeField] private Button     openX2Button;
    [SerializeField] private Button     closeButton;

    private ChestRewardSystem.RewardEntry pendingReward;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        panel.SetActive(false);
        openButton.onClick.AddListener(() => StartCoroutine(OpenRoutine(1)));
        openX2Button.onClick.AddListener(OnOpenX2Clicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    // ──────────────────────────────────────────────
    // Public API (called by BossEnemy)
    // ──────────────────────────────────────────────

    public void ShowChest()
    {
        pendingReward = ChestRewardSystem.Instance?.RollReward();

        rewardText.text = "Tap to open your reward!";
        openButton.gameObject.SetActive(true);
        openButton.interactable = true;
        openX2Button.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

        panel.SetActive(true);
        Time.timeScale = 0f; // pause game while chest is shown
    }

    // ──────────────────────────────────────────────
    // Button handlers
    // ──────────────────────────────────────────────

    void OnOpenX2Clicked()
    {
        AdServiceStub.Instance?.ShowRewardedAd(() => StartCoroutine(OpenRoutine(2)));
    }

    void OnCloseClicked()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        CoinManager.Instance?.CommitToTotal();
        GameManager.Instance?.EndLevel(true);
    }

    // ──────────────────────────────────────────────
    // Animation
    // ──────────────────────────────────────────────

    IEnumerator OpenRoutine(int multiplier)
    {
        openButton.gameObject.SetActive(false);
        openX2Button.gameObject.SetActive(false);

        if (chestAnimator != null)
            chestAnimator.SetTrigger("Open");

        // Wait for chest open animation (unscaled: game is paused)
        yield return new WaitForSecondsRealtime(1.5f);

        // Show reward
        if (pendingReward != null)
        {
            int amount = Random.Range(pendingReward.minAmount, pendingReward.maxAmount + 1) * multiplier;
            rewardText.text = $"+{amount} {pendingReward.rewardName}!";
            ChestRewardSystem.Instance?.GrantReward(pendingReward, multiplier);
        }
        else
        {
            rewardText.text = "You found... nothing? (configure reward table)";
        }

        // Only offer x2 if this was a standard open (not already doubled)
        openX2Button.gameObject.SetActive(multiplier == 1);
        closeButton.gameObject.SetActive(multiplier > 1 || pendingReward == null);
    }
}
