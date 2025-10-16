using UnityEngine;

public class BallMovement : MonoBehaviour
{

    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnCollisionEnter(Collision collision)
    {
        Vector3 forceDirection = -collision.contacts[0].normal;

        forceDirection.y = 0f;
        forceDirection.Normalize();

        float bounceStrength = 0.5f;
        rb.AddForce(forceDirection * bounceStrength, ForceMode.Impulse);
    }
        
    void FixedUpdate()
{
    // Keep Y position fixed at initial height
    Vector3 velocity = rb.linearVelocity;
    velocity.y = 0f;
    rb.linearVelocity = velocity;

    Vector3 pos = rb.position;
    pos.y = 0f;
    rb.position = pos;
}
}
