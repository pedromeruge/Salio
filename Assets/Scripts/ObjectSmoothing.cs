using System;
using UnityEngine;

public class ObjectSmoothing : MonoBehaviour
{
    [Header("Smoothing")]
    [Range(0f, 20f)]
    [SerializeField] private float positionSmoothSpeed = 8f;

    [Range(0f, 20f)]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    
    [Header("Deadzone (Jitter Filtering)")]
    [Tooltip("Ignore horizontal (XZ) position changes smaller than this (in meters)")]
    [SerializeField] private float horizontalDeadzone = 0.002f; // 2mm
    
    [Tooltip("Ignore vertical (Y) position changes smaller than this (in meters)")]
    [SerializeField] private float verticalDeadzone = 0.001f; // 1mm - usually less jitter vertically
    
    [Tooltip("Ignore rotation changes smaller than this (in degrees)")]
    [SerializeField] private float rotationDeadzone = 0.5f; // 0.5 degrees
    
    [Header("References")]
    [SerializeField] private Transform vuforiaTrackedObject; // vuforia image target transform
    [SerializeField] private Transform smoothedObject; // object to be smoothed (child of image target)
    
    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;
    private bool isInitialized = false;
    
    void Start()
    {
        if (vuforiaTrackedObject == null || smoothedObject == null)
        {
            Debug.LogError("References not assigned!");
            return;
        }

        // initialize with current tracked position
        smoothedPosition = vuforiaTrackedObject.position;
        smoothedRotation = vuforiaTrackedObject.rotation;
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized || vuforiaTrackedObject == null || smoothedObject == null) {
            return;
        }

        // # Position: check if position change exceeds deadzone
        Vector3 targetPosition = vuforiaTrackedObject.position;
        
        // Check horizontal (XZ) and vertical (Y) separately
        Vector2 currentXZ = new Vector2(smoothedPosition.x, smoothedPosition.z);
        Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);
        
        float horizontalDelta = Vector2.Distance(currentXZ, targetXZ);
        float verticalDelta = Mathf.Abs(smoothedPosition.y - targetPosition.y);

        Console.WriteLine("Horizontal delta: " + horizontalDelta + ", Vertical delta: " + verticalDelta);
        
        // apply smoothing to components that exceed their deadzone
        Vector3 newPosition = smoothedPosition;
        
        if (horizontalDelta > horizontalDeadzone)
        {
            // Smooth XZ
            newPosition.x = Mathf.Lerp(smoothedPosition.x, targetPosition.x, Time.deltaTime * positionSmoothSpeed);
            newPosition.z = Mathf.Lerp(smoothedPosition.z, targetPosition.z, Time.deltaTime * positionSmoothSpeed);
        }
        
        if (verticalDelta > verticalDeadzone)
        {
            // Smooth Y
            newPosition.y = Mathf.Lerp(smoothedPosition.y, targetPosition.y, Time.deltaTime * positionSmoothSpeed);
        }
        
        smoothedPosition = newPosition;

        // # Rotation: check if rotation change exceeds deadzone
        float rotationDelta = Quaternion.Angle(smoothedRotation, vuforiaTrackedObject.rotation);
        Console.WriteLine("Rotation delta: " + rotationDelta);
        if (rotationDelta > rotationDeadzone) {
            // smooth rotation
            smoothedRotation = Quaternion.Slerp(
                smoothedRotation,
                vuforiaTrackedObject.rotation,
                Time.deltaTime * rotationSmoothSpeed
            );
        }
        
        smoothedObject.position = smoothedPosition;
        smoothedObject.rotation = smoothedRotation;
    }
    
    // reset smoothing when tracking is lost/regained
    public void ResetSmoothing()
    {
        if (vuforiaTrackedObject != null)
        {
            smoothedPosition = vuforiaTrackedObject.position;
            smoothedRotation = vuforiaTrackedObject.rotation;
        }
    }
}