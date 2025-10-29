using UnityEngine;

public class ObjectSmoothing : MonoBehaviour
{
    [Header("Smoothing (EMA Alpha)")]
    [Range(0.01f, 0.99f)]
    [SerializeField] private float positionAlpha = 0.2f; // Lower = smoother (less responsive to jitter)
    
    [Range(0.01f, 0.99f)]
    [SerializeField] private float rotationAlpha = 0.2f;

    [Header("Deadzone (Jitter Filtering)")]
    [Tooltip("Ignore horizontal (XZ) position changes smaller than this (in meters)")]
    [SerializeField] private float horizontalDeadzone = 0.002f; // 2mm
    
    [Tooltip("Ignore vertical (Y) position changes smaller than this (in meters)")]
    [SerializeField] private float verticalDeadzone = 0.001f; // 1mm - usually less jittery vertically
    
    [Tooltip("Ignore rotation changes smaller than this (in degrees)")]
    [SerializeField] private float rotationDeadzone = 0.5f; // 0.5 degrees

    [Header("References")]
    [SerializeField] private Transform ghostTrackedObject; // the object that provides the marker raw position

    [Header("Visibility")]
    [SerializeField] private bool hideWhenNotTracking = true; // hide this object while not tracking the marker?

    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;
    private bool isInitialized = false;
    private Renderer[] renderers; // Cache all children renderers

    void Awake( )
    {
        renderers = GetComponentsInChildren<Renderer>();

        // start hidden if hideWhenNotTracking is enabled
        if (hideWhenNotTracking)
        {
            SetVisible(false);
        }
        else
        {
            isInitialized = true; // always initialized if not hiding
        }
    }
    void Start()
    {
        if (ghostTrackedObject == null)
        {
            Debug.LogError("ObjectSmoothing: Ghost Tracked Object not assigned!", this);
            return;
        }

        // Initialize with current ghost position
        smoothedPosition = ghostTrackedObject.position;
        smoothedRotation = ghostTrackedObject.rotation;
        
        Debug.Log("ObjectSmoothing initialized on " + name);
    }

    void LateUpdate() // lateUpdate ensures Vuforia object updates first
    {
        if (!isInitialized || ghostTrackedObject == null) return;

        Vector3 rawPosition = ghostTrackedObject.position;
        Quaternion rawRotation = ghostTrackedObject.rotation;

        Vector2 currentXZ = new Vector2(smoothedPosition.x, smoothedPosition.z);
        Vector2 rawXZ = new Vector2(rawPosition.x, rawPosition.z);
        float horizontalDelta = Vector2.Distance(currentXZ, rawXZ);
        float verticalDelta = Mathf.Abs(smoothedPosition.y - rawPosition.y);

        Debug.Log($"Pos Delta - H:{horizontalDelta:F4}, V:{verticalDelta:F4}");

        Vector3 newSmoothedPos = smoothedPosition;

        // check if we exceed deadzone in any axis
        if (horizontalDelta > horizontalDeadzone || verticalDelta > verticalDeadzone)
        {
            Debug.Log("Exceeding position deadzone");
            newSmoothedPos = Vector3.Lerp(smoothedPosition, rawPosition, positionAlpha);
        }
        smoothedPosition = newSmoothedPos;

        // check if exceed rotation deadzone check
        float rotationDelta = Quaternion.Angle(smoothedRotation, rawRotation);

        Debug.Log($"Rot Delta: {rotationDelta:F2}");

        if (rotationDelta > rotationDeadzone)
        {
            Debug.Log("Exceeding rotation deadzone");
            smoothedRotation = Quaternion.Slerp(smoothedRotation, rawRotation, rotationAlpha);
        }

        transform.position = smoothedPosition;
        transform.rotation = smoothedRotation;
    }

    // set visibility of all children renderers
    private void SetVisible(bool visible)
    {
        if (renderers == null) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }

        Debug.Log($"{name} visibility set to {visible}");
    }

    // Public reset for tracking events
    public void ResetSmoothing()
    {
        if (ghostTrackedObject != null)
        {
            smoothedPosition = ghostTrackedObject.position;
            smoothedRotation = ghostTrackedObject.rotation;
            transform.position = smoothedPosition;
            transform.rotation = smoothedRotation;
            Debug.Log("ObjectSmoothing reset on tracking change");
        }
    }

    public void onTrackingFound()
    {
        isInitialized = true;
        ResetSmoothing();

        if (hideWhenNotTracking)
        {
            SetVisible(true);
        }
    }
    
    public void onTrackingLost()
    {
        isInitialized = false;

        if (hideWhenNotTracking)
        {
            SetVisible(false);
        }   
    }
}