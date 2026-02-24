using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-centre XP progress bar (yellow).
/// Fills from 0 to 1 based on XP toward the next upgrade threshold.
///
/// Inspector Setup:
///   - fillImage: the foreground Image (fill method = Horizontal, anchor = left)
///   - Set this GameObject to stretch horizontally at the top of the HUD Canvas
///   - Recommended: bar height ~8–12 px, full width, top-centre anchor
/// </summary>
public class XPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Color xpColor = new Color(1f, 0.85f, 0f); // golden yellow

    void Start()
    {
        if (fillImage != null) fillImage.color = xpColor;

        if (XPSystem.Instance != null)
        {
            XPSystem.Instance.OnXPChanged += UpdateBar;
            UpdateBar(0, XPSystem.Instance.NextThreshold);
        }
    }

    void OnDestroy()
    {
        if (XPSystem.Instance != null)
            XPSystem.Instance.OnXPChanged -= UpdateBar;
    }

    void UpdateBar(int currentXP, int nextThreshold)
    {
        if (fillImage == null) return;
        float fill = nextThreshold > 0 ? (float)currentXP / nextThreshold : 1f;
        fillImage.fillAmount = Mathf.Clamp01(fill);
    }
}
