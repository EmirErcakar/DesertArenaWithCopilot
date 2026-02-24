using UnityEngine;

/// <summary>
/// Moves the player using floating joystick input.
///
/// Inspector Setup:
///   - moveSpeed: default 5
///   - Requires a Rigidbody (freeze rotation X, Z) on the same GameObject
///   - Tag this GameObject as "Player"
///   - FloatingJoystick is found automatically in the scene
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float deadzone = 0.1f;

    private Rigidbody rb;
    private FloatingJoystick joystick;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        joystick = FindFirstObjectByType<FloatingJoystick>();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }
        Move();
    }

    void Move()
    {
        if (joystick == null) return;

        Vector2 input = joystick.Direction;
        if (input.magnitude < deadzone)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Map joystick XY -> world XZ (top-down)
        Vector3 move = new Vector3(input.x, 0f, input.y) * (moveSpeed * input.magnitude);
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // Face movement direction
        if (move.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
    }

    /// <summary>Called by UpgradeManager when MoveSpeed stat is upgraded.</summary>
    public void ApplyMoveSpeedBonus(float bonus)
    {
        moveSpeed += bonus;
    }
}
