using Unity.VisualScripting;
using UnityEngine;

public enum Polarity { Attract, Repel }
public class MagnetAnchor : MonoBehaviour
{
    [Header("Magnet Settings")]
    public Polarity polarity = Polarity.Attract;
    public float strength = 10f;
    public float range = 5f; // distance up to which magnet has effect
    public float falloff = 2f; // how quickly strength decreases with distance // Higher = sharper dropoff (e.g., 1=linear, 2=inverse square)

    // Events for tracking state // can be used later by ball
    public System.Action<MagnetAnchor, bool> OnTrackingChanged;
    
    public bool isTracking { get; private set; } = false;

    [Header("Debug")]
    public bool hideGizmos = false;

    // ### NOTE: functions in below section should always be assigned to Vuforia "Default Observer Event Handler" script, which exists in the current object (if it is the marker gameObject) or nearest parent (if it is nested as child of the marker gameObject) ###
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


    // ##### 
    private void OnDrawGizmosSelected()
    {
        if (hideGizmos) return;
        // Visualize magnet range in editor
        Gizmos.color = (polarity == Polarity.Attract) ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}