using UnityEngine;

/// <summary>
/// Angled top-down camera that smoothly follows the player.
///
/// Inspector Setup:
///   - target: drag the Player transform here (or leave null to auto-find by tag "Player")
///   - offset: recommended (0, 14, -8) for a nice angled view
///   - smoothSpeed: 8 works well
///   - lookDownAngle: 60 degrees gives a good top-down angle
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -8f);
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float lookDownAngle = 60f;

    void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
