using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{

    public Transform mazeTransform; // maze marker
    [Header("Gravity Settings")]
    [SerializeField] private float gravityMagnitude = 5.0f;
    [SerializeField] private float gravitySmoothness = 5.0f;

    private Rigidbody rb;
    private Vector3 currentGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // force properties, if not done in editor
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (mazeTransform == null || rb.isKinematic) return;

        // calculate gravity direction based on maze orientation
        Vector3 targetGravity = -mazeTransform.up * gravityMagnitude;

        // Smooth transition to avoid jittering
        currentGravity = Vector3.Lerp(currentGravity, targetGravity, Time.fixedDeltaTime * gravitySmoothness);

        // Apply gravity force
        rb.AddForce(currentGravity, ForceMode.Acceleration);
    }

    // function called to reset ball local position and velocity
    public void ResetToSpawn(Vector3 spawnWorldPosition)
    {
        // reset velocities only if not kinematic, it gives warning otherwise
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // set world position directly
        transform.position = spawnWorldPosition;
        transform.rotation = Quaternion.identity;

        // force physics to sync
        Physics.SyncTransforms();

        Debug.Log($"Ball reset to world position: {spawnWorldPosition}");
        Debug.Log($"Ball actual position: {transform.position}");
        Debug.Log($"Ball local position: {transform.localPosition}");
    }
}