using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{

    public Transform mazeTransform; // maze marker

    [Header("Gravity Settings (maze)")]
    [SerializeField] private float gravityMagnitude = 5.0f;
    [SerializeField] private float gravitySmoothness = 5.0f;

    [Header("Magnet Settings")]
    [SerializeField] private LayerMask magnetLayerMask = -1; // which layers contain magnets, by default all

    
    private Rigidbody rb;
    private Vector3 currentGravity;
    [SerializeField] private MagnetAnchor[] influencingMagnets ; // magnets that affect the ball

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // force properties, if not done in editor
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (mazeTransform == null || rb.isKinematic) return;

        // calculate gravity direction based on maze orientation
        Vector3 mazeGravity = -mazeTransform.up * gravityMagnitude;
        Vector3 magnetsGravity = CalculateMagneticForces();
        Vector3 targetGravity = mazeGravity + magnetsGravity;
        // Smooth transition to avoid jittering
        currentGravity = Vector3.Lerp(currentGravity, targetGravity, Time.fixedDeltaTime * gravitySmoothness);

        // Apply gravity force
        rb.AddForce(currentGravity, ForceMode.Acceleration);
    }

    // New: Calculate and apply summed forces from all tracked magnets
    private Vector3 CalculateMagneticForces()
    {
        Vector3 totalMagnetForce = Vector3.zero;

        // if no magnets assigned, fallback to maze gravity
        if (influencingMagnets == null || influencingMagnets.Length == 0)
        {
            Debug.Log("No influencing magnets assigned to ball");
            totalMagnetForce = -mazeTransform.up * gravityMagnitude; // fallback to maze gravity
        }

        foreach (MagnetAnchor magnet in influencingMagnets)
        {
            if (!magnet.isTracking) continue; // if magnet not currently tracked, ignore

            // filter only valid layers
            if (!checkLayerInMask(magnetLayerMask, magnet.gameObject.layer)) continue;

            float dist = Vector3.Distance(transform.position, magnet.transform.position);
            if (dist > magnet.range || dist < 0.01f) continue; // skip magnets out-of-range or too close

            Debug.Log("Magnet influencing ball: " + magnet.name + " at distance " + dist);
            // direction: from ball to magnet (for attract) or opposite (repel)
            Vector3 direction = (magnet.transform.position - transform.position).normalized;
            if (magnet.polarity == Polarity.Repel)
            {
                direction = -direction;
            }

            // distance falloff
            float forceMagnitude = magnet.strength / Mathf.Pow(dist, magnet.falloff);

            Vector3 force = direction * forceMagnitude;

            // project force to maze plane (XZ relative to maze, ignore Y for surface play)
            if (mazeTransform != null)
            {
                Vector3 localForce = mazeTransform.InverseTransformDirection(force);
                localForce.y = 0f; // flatten to plane domain
                force = mazeTransform.TransformDirection(localForce);
            }

            totalMagnetForce += force;
        }

        Debug.Log($"Applied magnet force to ball: {totalMagnetForce} (from {influencingMagnets.Count(m => m.isTracking)} magnets)");

        return totalMagnetForce;
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
    
    // check if a layer is in a layermask
    bool checkLayerInMask (LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}