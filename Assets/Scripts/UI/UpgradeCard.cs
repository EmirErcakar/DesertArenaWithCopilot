using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Single upgrade card that flips from back to front with a scale animation.
///
/// Inspector Setup:
///   - cardBack:    the back-face GameObject (shown before flip)
///   - cardFront:   the front-face GameObject (shown after flip)
///   - frameImage:  border Image on the front face (colour set by UpgradeDefinition)
///   - iconImage:   icon Image on the front face
///   - nameText:    TMP label for the upgrade name
///   - descText:    TMP label for the description
///   - selectButton: the clickable button on the front face
///   - flipDuration: 0.35 s per half looks great
///
/// Scene Prefab hierarchy:
///   UpgradeCard (this script + Button)
///     ├─ CardBack  (Image — any placeholder back art)
///     └─ CardFront
///          ├─ Frame     (Image — coloured border)
///          ├─ Icon      (Image)
///          ├─ NameText  (TextMeshPro)
///          └─ DescText  (TextMeshPro)
/// </summary>
public class UpgradeCard : MonoBehaviour
{
    [Header("Card Faces")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFront;

    [Header("Front Face")]
    [SerializeField] private Image    frameImage;
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button   selectButton;

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.35f;

    private UpgradeDefinition            upgradeData;
    private Action<UpgradeDefinition>    onSelected;

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    /// <summary>
    /// Configure this card and start the flip-in animation.
    /// delay: stagger offset so cards don't all flip at once.
    /// </summary>
    public void Setup(UpgradeDefinition data, Action<UpgradeDefinition> callback, float delay = 0f)
    {
        upgradeData = data;
        onSelected  = callback;

        // Reset to back face
        cardBack.SetActive(true);
        cardFront.SetActive(false);
        transform.localScale = Vector3.one;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardClicked);
        selectButton.interactable = true;

        StopAllCoroutines();
        StartCoroutine(FlipRoutine(delay));
    }

    // ──────────────────────────────────────────────
    // Animation
    // ──────────────────────────────────────────────

    IEnumerator FlipRoutine(float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        // First half: squish to flat (scale X → 0)
        yield return ScaleHalf(Vector3.one, new Vector3(0f, 1f, 1f));

        // Swap faces at midpoint
        cardBack.SetActive(false);
        cardFront.SetActive(true);
        PopulateFront();

        // Second half: expand back (scale X → 1)
        yield return ScaleHalf(new Vector3(0f, 1f, 1f), Vector3.one);
    }

    IEnumerator ScaleHalf(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / flipDuration;
            transform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        transform.localScale = to;
    }

    // ──────────────────────────────────────────────
    // Front face population
    // ──────────────────────────────────────────────

    void PopulateFront()
    {
        if (upgradeData == null) return;

        if (nameText  != null) nameText.text   = upgradeData.upgradeName;
        if (descText  != null) descText.text   = upgradeData.description;
        if (iconImage != null && upgradeData.icon != null)
            iconImage.sprite = upgradeData.icon;
        if (frameImage != null)
            frameImage.color = upgradeData.GetFrameColor();
    }

    // ──────────────────────────────────────────────
    // Input
    // ──────────────────────────────────────────────

    void OnCardClicked()
    {
        selectButton.interactable = false;
        onSelected?.Invoke(upgradeData);
    }
}
