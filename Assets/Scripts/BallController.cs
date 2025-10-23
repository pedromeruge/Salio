using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{

    public Transform mazeTransform; // maze marker
    public AnchorGravityScript gravityAnchor; // anchor that controls gravity direction

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMagnitude = 5.0f;
    [SerializeField] private float gravitySmoothness = 5.0f;
    [SerializeField] private bool useGravityAnchor = true;

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
        Vector3 targetGravity;

        if (gravityAnchor != null && gravityAnchor.IsTracking)
        {
            targetGravity = CalculateGravityFromAnchor();
        }
        else if (!useGravityAnchor)
        {
            // maze-based gravity
            targetGravity = -mazeTransform.up * gravityMagnitude;

        }
        else
        {
            // No gravity if anchor is required but not tracking
            targetGravity = Vector3.zero;
        }

        // Smooth transition to avoid jittering
        currentGravity = Vector3.Lerp(currentGravity, targetGravity, Time.fixedDeltaTime * gravitySmoothness);

        // Apply gravity force
        rb.AddForce(currentGravity, ForceMode.Acceleration);
    }

    // calculate gravity vector based on anchor position
    private Vector3 CalculateGravityFromAnchor()
    {
        // get anchor position relative to maze center in maze's local space
        Vector3 anchorLocalPos = mazeTransform.InverseTransformPoint(gravityAnchor.transform.position);

        // project onto XZ plane (ignore Y, keep it on maze surface)
        Vector2 anchorDirection2D = new Vector2(anchorLocalPos.x, anchorLocalPos.z);

        // if anchor is at center, use default maze gravity
        if (anchorDirection2D.sqrMagnitude < 0.001f)
        {
            return -mazeTransform.up * gravityMagnitude;
        }

        // normalize to get direction
        anchorDirection2D.Normalize();

        // convert back to 3D local space (on the XZ plane)
        Vector3 localGravityDirection = new Vector3(anchorDirection2D.x, 0f, anchorDirection2D.y);

        // transform to world space and apply magnitude
        Vector3 worldGravityDirection = mazeTransform.TransformDirection(localGravityDirection);

        return worldGravityDirection * gravityMagnitude;
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

        // Reset gravity smoothing
        currentGravity = Vector3.zero;
        
        // force physics to sync
        Physics.SyncTransforms();

        Debug.Log($"Ball reset to world position: {spawnWorldPosition}");
        Debug.Log($"Ball actual position: {transform.position}");
        Debug.Log($"Ball local position: {transform.localPosition}");
    }
    
    // draw gravity direction
    private void OnDrawGizmos()
    {
        if (rb != null && !rb.isKinematic && currentGravity.sqrMagnitude > 0.001f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, currentGravity.normalized * 0.1f);
        }
    }
}