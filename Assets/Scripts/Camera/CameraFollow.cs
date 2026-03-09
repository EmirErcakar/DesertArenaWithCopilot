using UnityEngine;

namespace DesertArena.Camera
{
    /// <summary>
    /// Smoothly follows the player with a configurable offset.
    /// Designed for a portrait-oriented mobile top-down view.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Target")]
        [SerializeField]
        [Tooltip("The transform to follow (usually the player).")]
        private Transform _target;

        [Header("Follow Settings")]
        [SerializeField]
        [Tooltip("Offset from the target (angled top-down for portrait mobile).")]
        private Vector3 _offset = new Vector3(0f, 12f, -8f);

        [SerializeField]
        [Tooltip("Approximate time (seconds) for the camera to reach the target position.")]
        private float _smoothTime = 0.2f;

        #endregion

        #region Private Fields

        private Vector3 _currentVelocity;

        #endregion

        #region Properties

        /// <summary>The transform being followed.</summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Target atanmadıysa Player tag'li objeyi otomatik bul
            if (_target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _target = playerObj.transform;
                    Debug.Log("[CameraFollow] Player otomatik olarak bulundu ve hedefe atandı");
                    SnapToTarget();
                }
                else
                {
                    Debug.LogWarning("[CameraFollow] 'Player' tag'li obje bulunamadı! Inspector'da Target atayın.");
                }
            }
            else
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;

            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref _currentVelocity, _smoothTime);

            // Always look at the target for a consistent top-down angle
            transform.LookAt(_target.position);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Instantly snaps the camera to the target position without smoothing.
        /// Useful when spawning or teleporting the player.
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            transform.position = _target.position + _offset;
            transform.LookAt(_target.position);
            _currentVelocity = Vector3.zero;
        }

        #endregion
    }
}
