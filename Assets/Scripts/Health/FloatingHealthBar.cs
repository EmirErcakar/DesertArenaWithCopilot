using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space HP bar that floats above a unit and always faces the camera.
///
/// Inspector Setup:
///   - healthComponent: drag the HealthComponent of the owning unit
///   - fillImage: the foreground fill Image in the world-space canvas (fillMethod = Horizontal)
///   - canvas: the world-space Canvas on this bar object
///   - offset: height above unit origin (default (0, 2, 0))
///   - hideWhenFull: hides bar when at full HP (default true)
///
/// Scene Setup:
///   Create as child of enemy/player:
///     [Enemy]
///       └─ HealthBar        (this script + a world Canvas)
///            ├─ Background  (Image, grey)
///            └─ Fill        (Image, green/red, fillMethod Horizontal)
/// </summary>
public class FloatingHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image           fillImage;
    [SerializeField] private Canvas          canvas;

    [Header("Settings")]
    [SerializeField] private Vector3 offset       = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool    hideWhenFull = true;

    private Transform cam;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged += UpdateBar;
            UpdateBar(healthComponent.CurrentHealth, healthComponent.MaxHealth);
        }
    }

    void LateUpdate()
    {
        // Keep offset relative to parent's world position
        transform.position = transform.parent != null
            ? transform.parent.position + offset
            : transform.position;

        // Billboard: face camera
        if (cam != null)
            transform.rotation = cam.rotation;
    }

    void UpdateBar(float current, float max)
    {
        if (fillImage == null) return;
        float ratio = max > 0f ? current / max : 0f;
        fillImage.fillAmount = Mathf.Clamp01(ratio);

        if (hideWhenFull && canvas != null)
            canvas.enabled = ratio < 0.999f;
    }

    void OnDestroy()
    {
        if (healthComponent != null)
            healthComponent.OnHealthChanged -= UpdateBar;
    }
}
