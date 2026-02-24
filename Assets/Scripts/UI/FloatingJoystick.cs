using UnityEngine;
using UnityEngine.EventSystems;

namespace DesertArena.UI
{
    /// <summary>
    /// Touch-based floating joystick that spawns at the touch position.
    /// Attach to a Canvas child with a RectTransform. Implements pointer
    /// event interfaces for cross-platform input.
    /// </summary>
    public class FloatingJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Serialized Fields

        [Header("Components")]
        [SerializeField] [Tooltip("Background circle RectTransform.")]
        private RectTransform background;

        [SerializeField] [Tooltip("Handle (knob) circle RectTransform.")]
        private RectTransform handle;

        [Header("Settings")]
        [SerializeField] [Tooltip("Maximum radius the handle can move from center (pixels).")]
        private float clampRadius = 75f;

        [SerializeField] [Tooltip("Input magnitudes below this threshold are treated as zero.")]
        private float deadzone = 0.1f;

        #endregion

        #region Private Fields

        private Canvas _parentCanvas;
        private Camera _canvasCamera;
        private Vector2 _inputDirection;
        private bool _isDragging;

        #endregion

        #region Properties

        /// <summary>Horizontal axis value (-1 to 1).</summary>
        public float Horizontal => _inputDirection.x;

        /// <summary>Vertical axis value (-1 to 1).</summary>
        public float Vertical => _inputDirection.y;

        /// <summary>Normalized direction vector from the joystick.</summary>
        public Vector2 InputDirection => _inputDirection;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas != null &&
                _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _canvasCamera = _parentCanvas.worldCamera;
            }

            HideJoystick();
        }

        #endregion

        #region Pointer Handlers

        /// <summary>
        /// Called when the player touches the joystick area. Positions the
        /// joystick visuals at the touch point.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (background == null || handle == null) return;

            background.gameObject.SetActive(true);
            handle.gameObject.SetActive(true);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, eventData.position,
                _canvasCamera, out Vector2 localPoint);

            background.anchoredPosition = localPoint;
            handle.anchoredPosition = localPoint;

            _isDragging = true;
        }

        /// <summary>
        /// Called while the player drags their finger. Updates the handle
        /// position and calculates the input direction with clamping.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || background == null || handle == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, eventData.position,
                _canvasCamera, out Vector2 localPoint);

            Vector2 offset = localPoint - background.anchoredPosition;

            if (offset.magnitude > clampRadius)
            {
                offset = offset.normalized * clampRadius;
            }

            handle.anchoredPosition = background.anchoredPosition + offset;

            Vector2 direction = offset / clampRadius;

            if (direction.magnitude < deadzone)
            {
                direction = Vector2.zero;
            }

            _inputDirection = direction;
        }

        /// <summary>
        /// Called when the player lifts their finger. Resets input and hides
        /// the joystick visuals.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _inputDirection = Vector2.zero;
            HideJoystick();
        }

        #endregion

        #region Private Methods

        private void HideJoystick()
        {
            if (background != null) background.gameObject.SetActive(false);
            if (handle != null) handle.gameObject.SetActive(false);
        }

        #endregion
    }
}
