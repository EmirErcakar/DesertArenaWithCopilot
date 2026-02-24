using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Floating joystick that spawns wherever the player first touches (non-UI areas only).
/// UI elements with Raycast Target = true will consume touches and prevent joystick spawning.
///
/// Inspector Setup:
///   - Attach this component to a full-screen transparent Image (the joystick input panel)
///   - joystickContainer: the outer circle RectTransform (set inactive initially)
///   - knob: the inner draggable circle RectTransform
///   - clampRadius: 60 pixels works well for most screens (adjust per resolution)
///   - Canvas should be Screen Space - Overlay with a Canvas Scaler (Scale With Screen Size,
///     reference 1080x1920 for portrait)
/// </summary>
[RequireComponent(typeof(Image))]
public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform joystickContainer;  // outer circle
    [SerializeField] private RectTransform knob;               // inner draggable circle

    [Header("Settings")]
    [SerializeField] private float clampRadius = 60f;   // UI units
    [SerializeField] private float deadzone     = 0.15f; // normalised 0..1

    /// <summary>Normalised direction [-1..1, -1..1]. Zero when inside deadzone.</summary>
    public Vector2 Direction { get; private set; }

    private int    activeTouchId = -1;
    private Canvas parentCanvas;
    private RectTransform canvasRect;

    void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        canvasRect   = parentCanvas.GetComponent<RectTransform>();

        // Make the background image fully transparent so it doesn't visually appear
        var img = GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);

        HideJoystick();
    }

    // ──────────────────────────────────────────────
    // IPointer/IDrag callbacks
    // ──────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activeTouchId != -1) return; // already tracking one touch
        activeTouchId = eventData.pointerId;
        joystickContainer.gameObject.SetActive(true);
        MoveContainerTo(eventData.position);
        UpdateKnob(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activeTouchId) return;
        UpdateKnob(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activeTouchId) return;
        activeTouchId = -1;
        Direction = Vector2.zero;
        HideJoystick();
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    void MoveContainerTo(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            GetEventCamera(),
            out Vector2 local);
        joystickContainer.anchoredPosition = local;
    }

    void UpdateKnob(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickContainer,
            screenPos,
            GetEventCamera(),
            out Vector2 local);

        Vector2 clamped   = Vector2.ClampMagnitude(local, clampRadius);
        knob.anchoredPosition = clamped;

        Vector2 normalised = clamped / clampRadius;
        Direction = normalised.magnitude > deadzone ? normalised : Vector2.zero;
    }

    void HideJoystick()
    {
        joystickContainer.gameObject.SetActive(false);
        knob.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
    }

    Camera GetEventCamera()
    {
        return parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : parentCanvas.worldCamera;
    }
}
