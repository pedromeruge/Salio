using UnityEngine;

public class DebugCollider : MonoBehaviour
{

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision detected with " + other.gameObject.name);
        Debug.Log("Contact points:");
        foreach (ContactPoint contact in other.contacts) {
            Debug.Log("Point: " + contact.point + ", Normal: " + contact.normal);
        }
    }
}
