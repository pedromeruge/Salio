using UnityEngine;

public class AnchorGravityScript : MonoBehaviour
{
    [Header("Anchor Settings")]
    [SerializeField] private bool isTracking = false;
    
    // Events for tracking state
    public System.Action<AnchorGravityScript, bool> OnTrackingChanged;

    public bool IsTracking => isTracking;

    // Called by Vuforia tracking events
    public void OnTrackingFound()
    {
        isTracking = true;
        OnTrackingChanged?.Invoke(this, true);
        Debug.Log("Gravity Anchor tracking found");
    }

    public void OnTrackingLost()
    {
        isTracking = false;
        OnTrackingChanged?.Invoke(this, false);
        Debug.Log("Gravity Anchor tracking lost");
    }
}