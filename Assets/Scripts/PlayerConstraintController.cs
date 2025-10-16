using Unity.VisualScripting;
using UnityEngine;

public class PlayerConstraintController : MonoBehaviour
{
    [Header("Tracker References")]
    [SerializeField] private Transform arenaMarker;     // The main arena marker (defines world space)
    [SerializeField] private Transform playerMarker;    // The player marker (controls cube position)

    [Header("Arena Bounds")]
    [SerializeField] private Transform playerAreaBoundary;

    [Header("Smoothing")]
    [Range(0f, 20f)]
    [SerializeField] private float smoothSpeed = 8f;    // Higher = snappier movement, 0 = teleport

    private Vector3 smoothedPos;
    private Quaternion smoothedRot;

    private Transform playerCube; // cube representing the player

    // player area related
    private Vector2 playerAreaSize = new Vector2(1f, 1f); // x = width, y = depth
    private Vector2 playerAreaCenter = Vector2.zero;

    void Start()
    {
        playerCube = this.transform;
        smoothedPos = playerCube.position;
        smoothedRot = playerCube.rotation;


        if (playerAreaBoundary != null && arenaMarker != null) {
            playerAreaSize = new Vector2(
                // NOTE: lossy scale accounts for all scaling in the hierarchy, whereas localScale is just the object's own scale
                // unity plane default size is 10×10 units, so scale 1x1x1 gives 10x10 world size ???? valve pls fix
                playerAreaBoundary.lossyScale.x * 10.0f,
                playerAreaBoundary.lossyScale.z * 10.0f
            );
        
            // Convert boundary's world position to arena's local space
            // Vector3 localCenter = arenaMarker.InverseTransformPoint(playerAreaBoundary.position);
            playerAreaCenter = new Vector2(playerAreaBoundary.position.x, playerAreaBoundary.position.z);
        }
    }

    void Update()
    {
        if (arenaMarker == null || playerMarker == null)
            return;

        // # Position logic:
        // compute player position in arena local coordinate system
        Vector3 localPos = arenaMarker.InverseTransformPoint(playerMarker.position);

        // clamp position to arena bounds 
        float halfX = playerAreaSize.x / 2f;
        float halfZ = playerAreaSize.y / 2f;
        localPos.x = Mathf.Clamp(localPos.x, -halfX + playerAreaCenter.x, halfX + playerAreaCenter.x);
        localPos.z = Mathf.Clamp(localPos.z, -halfZ + playerAreaCenter.y, halfZ + playerAreaCenter.y);

        // lock y position, to always be at same height in relation to arena, instead of independent y position for player and arena
        localPos.y = 0.0f; // NOTE: can increase this to make player float above arena

        // convert back to world space
        Vector3 clampedWorldPos = arenaMarker.TransformPoint(localPos);

        // smooth position motion to reduce Vuforia jitter
        smoothedPos = Vector3.Lerp(smoothedPos, clampedWorldPos, Time.deltaTime * smoothSpeed);
        playerCube.position = smoothedPos;

        // # Rotation logic:
        // match arena's X (pitch) and Z rotation (roll), to keep box appearing 2d over arena even when arena is tilted
        // dont match Y (yaw) rotation, that is controlled by player movement so he can rotate in 2d space

        // get the player marker's rotation relative to the arena
        Quaternion localRotation = Quaternion.Inverse(arenaMarker.rotation) * playerMarker.rotation;
        
        // extract only the y axis rotation from the local rotation (rotation around arena surface normal axis)
        Vector3 localEuler = localRotation.eulerAngles;
        Quaternion localYRotation = Quaternion.Euler(0f, localEuler.y, 0f);

        // combine: arena full rotation (tilt/roll) + player y rotation relative to arena
        Quaternion targetRotation = arenaMarker.rotation * localYRotation;
        
        // Smooth rotation
        smoothedRot = Quaternion.Slerp(smoothedRot, targetRotation, Time.deltaTime * smoothSpeed);
        playerCube.rotation = smoothedRot;
    }
}
