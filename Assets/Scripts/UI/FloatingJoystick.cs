using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesertArena.UI
{
    /// <summary>
    /// Touch-based floating joystick that spawns at the touch position.
    /// Attach to a Canvas child with a RectTransform. Implements pointer
    /// event interfaces for cross-platform input.
    /// Requires an Image component as a raycast target for pointer events.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FloatingJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Serialized Fields

        [Header("Components")]
        [SerializeField] [Tooltip("Background circle RectTransform (otomatik oluşturulur).")]
        private RectTransform background;

        [SerializeField] [Tooltip("Handle (knob) circle RectTransform (otomatik oluşturulur).")]
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
            // RectTransform'un tüm Canvas alanını kaplamasını sağla
            EnsureFullScreenRectTransform();

            // Image bileşenini raycast target olarak ayarla
            EnsureRaycastImage();

            // Canvas ve kamera referanslarını al
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas != null &&
                _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _canvasCamera = _parentCanvas.worldCamera;
            }

            // Background ve Handle yoksa otomatik oluştur
            if (background == null || handle == null)
            {
                CreateJoystickVisuals();
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

        /// <summary>
        /// RectTransform'un tüm parent alanını kaplamasını sağlar (joystick
        /// ekranın her yerinde çalışır).
        /// </summary>
        private void EnsureFullScreenRectTransform()
        {
            RectTransform rt = transform as RectTransform;
            if (rt == null) return;

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Image bileşenini şeffaf raycast target olarak ayarlar.
        /// Bu olmadan pointer event'leri (dokunma) çalışmaz.
        /// </summary>
        private void EnsureRaycastImage()
        {
            Image img = GetComponent<Image>();
            if (img == null) return;

            img.raycastTarget = true;

            // Tamamen şeffaf yap — joystick alanı görünmez ama dokunulabilir
            img.color = new Color(0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Inspector'da Background ve Handle atanmadıysa otomatik oluşturur.
        /// </summary>
        private void CreateJoystickVisuals()
        {
            // Background (dış daire)
            if (background == null)
            {
                GameObject bgObj = new GameObject("JoystickBackground");
                bgObj.transform.SetParent(transform, false);
                Image bgImg = bgObj.AddComponent<Image>();
                bgImg.color = new Color(1f, 1f, 1f, 0.3f);
                bgImg.raycastTarget = false;

                RectTransform bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.sizeDelta = new Vector2(clampRadius * 2.5f, clampRadius * 2.5f);
                background = bgRt;
            }

            // Handle (iç daire / knob)
            if (handle == null)
            {
                GameObject handleObj = new GameObject("JoystickHandle");
                handleObj.transform.SetParent(transform, false);
                Image handleImg = handleObj.AddComponent<Image>();
                handleImg.color = new Color(1f, 1f, 1f, 0.6f);
                handleImg.raycastTarget = false;

                RectTransform handleRt = handleObj.GetComponent<RectTransform>();
                handleRt.sizeDelta = new Vector2(clampRadius * 1f, clampRadius * 1f);
                handle = handleRt;
            }
        }

        #endregion
    }
}
