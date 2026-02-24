using UnityEngine;
using TMPro;

/// <summary>
/// Displays the "BIG BOSS SPAWNING IN 10..9..8.." countdown overlay.
///
/// Inspector Setup:
///   - panel:         root panel (full-screen overlay with dark semi-transparent background)
///   - countdownText: large TMP_Text centred on screen
///
/// WaveManager calls Show(), UpdateCount(n), Hide() in sequence.
/// </summary>
public class BossCountdownUI : MonoBehaviour
{
    public static BossCountdownUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   countdownText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        countdownText.text = "BIG BOSS SPAWNING IN 10";
    }

    public void UpdateCount(int seconds)
    {
        countdownText.text = $"BIG BOSS SPAWNING IN {seconds}";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
