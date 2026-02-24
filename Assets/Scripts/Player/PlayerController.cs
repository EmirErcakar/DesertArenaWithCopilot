using UnityEngine;

namespace DesertArena.Player
{
    /// <summary>
    /// Handles player movement via a floating joystick and rotates
    /// toward the current target. Auto-fires via WeaponController.
    /// Requires a CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerTargeting))]
    [RequireComponent(typeof(WeaponController))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input")]
        [SerializeField]
        [Tooltip("Reference to the on-screen floating joystick.")]
        private FloatingJoystick _joystick;

        [SerializeField]
        [Tooltip("Joystick deadzone — inputs below this magnitude are ignored.")]
        private float _deadzone = 0.1f;

        [Header("Movement")]
        [SerializeField]
        [Tooltip("Gravity applied per second when not grounded.")]
        private float _gravity = -9.81f;

        [Header("Rotation")]
        [SerializeField]
        [Tooltip("How fast the player rotates toward the target (degrees/sec).")]
        private float _rotationSpeed = 720f;

        #endregion

        #region Private Fields

        private CharacterController _characterController;
        private PlayerStats _stats;
        private PlayerTargeting _targeting;
        private WeaponController _weaponController;
        private Vector3 _velocity;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _targeting = GetComponent<PlayerTargeting>();
            _weaponController = GetComponent<WeaponController>();
        }

        private void Update()
        {
            if (_stats.IsDead) return;

            HandleMovement();
            HandleRotation();
            HandleAutoFire();
        }

        #endregion

        #region Private Methods

        private void HandleMovement()
        {
            // Read joystick input
            Vector2 raw = _joystick != null
                ? new Vector2(_joystick.Horizontal, _joystick.Vertical)
                : Vector2.zero;

            // Apply deadzone
            float magnitude = raw.magnitude;
            if (magnitude < _deadzone)
            {
                raw = Vector2.zero;
                magnitude = 0f;
            }

            // Clamp to unit circle
            if (magnitude > 1f)
            {
                raw /= magnitude;
                magnitude = 1f;
            }

            // Convert to world-space direction (top-down XZ plane)
            Vector3 moveDir = new Vector3(raw.x, 0f, raw.y);

            // Analog speed: small input = slow, far input = full speed
            float speed = _stats.MoveSpeed * magnitude;
            Vector3 horizontalMove = moveDir * speed;

            // Simple gravity
            if (_characterController.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;

            Vector3 finalMove = (horizontalMove + _velocity) * Time.deltaTime;
            _characterController.Move(finalMove);
        }

        private void HandleRotation()
        {
            // Face the current target if one exists
            Transform target = _targeting.CurrentTarget;
            if (target == null) return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        private void HandleAutoFire()
        {
            // Continuously fire when a target is in range
            if (_targeting.CurrentTarget != null)
            {
                _weaponController.TryFire();
            }
        }

        #endregion
    }

    /// <summary>
    /// Minimal floating joystick interface.
    /// Replace with your actual joystick implementation (e.g., from an Asset Store package).
    /// </summary>
    public class FloatingJoystick : MonoBehaviour
    {
        /// <summary>Horizontal axis value (-1 to 1).</summary>
        public float Horizontal { get; set; }

        /// <summary>Vertical axis value (-1 to 1).</summary>
        public float Vertical { get; set; }
    }
}
