using System.Collections;
using UnityEngine;
using Vuforia;

public class MazeController : MonoBehaviour
{
    [Header("Maze Setup")]
    public Transform spawnPoint;
    public BallController ball;

    [Header("Stability Settings")]
    [SerializeField] private float stabilityDuration = 0.3f;
    [SerializeField] private float positionThreshold = 0.01f;
    [SerializeField] private float rotationThreshold = 1f;

    [Header("Tracking Loss Protection")]
    [SerializeField] private float flickerTolerance = 0.2f; // time before considering it a real tracking loss


    [Header("Out of Bounds Settings")]
    [SerializeField] private float outOfBoundsYThreshold = 3f; // relative to maze position

    private bool isTracking = false;
    private bool ballSpawned = false;
    private Coroutine stabilityCheckCoroutine; // track stability of maze to spawn ball
    private Coroutine flickerProtectionCoroutine; // track flickering of maze to protect ball going out of bounds

    // Store ball state when tracking is lost
    private Vector3 savedBallVelocity;
    private Vector3 savedBallAngularVelocity;

    public void FixedUpdate()
    {
        // prevent ball going outside bounds
        if (Mathf.Abs(transform.position.y - ball.transform.position.y) > outOfBoundsYThreshold)
        {
            // reset ball to spawn point above maze
            Debug.Log("Ball out of bounds, resetting to spawn point");
            ball.ResetToSpawn(spawnPoint.position);
        }
    }
    public void OnTrackingFound()
    {
        isTracking = true;

        // cancel flicker protection if tracking came back quickly
        if (flickerProtectionCoroutine != null)
        {
            StopCoroutine(flickerProtectionCoroutine);
            flickerProtectionCoroutine = null;

            // restore ball physics if it was frozen due to flicker
            if (ballSpawned && ball != null)
            {
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null && rb.isKinematic)
                {
                    // restore velocities and show ball
                    ball.SetPhysicsEnabled(true);
                    rb.linearVelocity = savedBallVelocity;
                    rb.angularVelocity = savedBallAngularVelocity;
                    Debug.Log("Tracking restored - ball unfrozen");
                }
            }
            return; // dont respawn, just continue
        }

        ballSpawned = false;

        // when maze tracker found, stop any remaining stability check
        if (stabilityCheckCoroutine != null)
        {
            StopCoroutine(stabilityCheckCoroutine);
        }

        // start checking for stability again
        stabilityCheckCoroutine = StartCoroutine(WaitForStabilityAndSpawn());

        // notify manager
        ARGameManager.Instance.OnMazeTrackingChanged(this, true);
    }

    // function called on tracking found events from Vuforia
    public void OnTrackingLost() {
        // start flicker protection
        if (flickerProtectionCoroutine == null)
        {
            flickerProtectionCoroutine = StartCoroutine(HandleTrackingLossWithFlickerProtection());
        }
    }

    // handle tracking loss by protecting ball state while maze isnt stable again
    private IEnumerator HandleTrackingLossWithFlickerProtection()
    {
        // Wait to see if tracking comes back (flicker tolerance)
        yield return new WaitForSeconds(flickerTolerance);

        isTracking = false;

        // stop stability check if running
        if (stabilityCheckCoroutine != null)
        {
            StopCoroutine(stabilityCheckCoroutine);
            stabilityCheckCoroutine = null;
        }

        // freeze ball physics
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                // save current velocities
                savedBallVelocity = rb.linearVelocity;
                savedBallAngularVelocity = rb.angularVelocity;

                // freeze the ball
                ball.SetPhysicsEnabled(false);
                Debug.Log("Tracking lost - ball frozen");
            }
        }

        // notify manager
        ARGameManager.Instance.OnMazeTrackingChanged(this, false);
        flickerProtectionCoroutine = null;

    }

    // check when marker is stable before spawning ball
    private IEnumerator WaitForStabilityAndSpawn()
    {
        if (ball == null || spawnPoint == null)
        {
            Debug.LogWarning("Ball or SpawnPoint is null!");
            yield break;
        }
        
        ball.SetPhysicsEnabled(false); // hide and freeze ball until stable
        Debug.Log("Waiting for marker stability...");

        Vector3 lastPosition = transform.position;
        Quaternion lastRotation = transform.rotation;
        float stableTime = 0f;

        while (stableTime < stabilityDuration)
        {
            if (!isTracking)
            {
                Debug.Log("Tracking lost during stability check");
                yield break;
            }

            yield return new WaitForFixedUpdate();

            // check if marker moved
            float positionDelta = Vector3.Distance(transform.position, lastPosition);
            float rotationDelta = Quaternion.Angle(transform.rotation, lastRotation);

            // Debug.Log($"Position delta: {positionDelta:F4}, Rotation delta: {rotationDelta:F2}");

            if (positionDelta < positionThreshold && rotationDelta < rotationThreshold)
            {
                // marker is stable
                stableTime += Time.fixedDeltaTime;
            }
            else
            {
                // Marker moved, reset timer
                stableTime = 0f;
                Debug.Log("Marker moved");
            }

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        // marker is stable, spawn ball
        Debug.Log("Marker stable! Spawning ball...");
        ball.ResetToSpawn(spawnPoint.position);
        ballSpawned = true;

        // wait multiple frames for physics to fully settle
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // enable physics
        ball.SetPhysicsEnabled(true);

        Debug.Log("Ball physics enabled");
    }

    
}