using UnityEngine;
using System.Collections;

public class VideoBackgroundReparenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera ARCamera;

    [Header("Debug")]
    [SerializeField] private bool logReparent = true;

    private Camera mainCam; // cached main camera
    void Start()
    {
        if (ARCamera == null)
        {
            Debug.LogError("VideoBackgroundReparenter: AR Camera reference not assigned!", this);
            return;
        }
        mainCam = GetComponent<Camera>();
        StartCoroutine(ReparentBackground());
    }

    private IEnumerator ReparentBackground()
    {
        // wait for Vuforia to create VideoBackground (usually few frames)
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        // find the video background child (Vuforia names it "VideoBackground")
        Transform videoBg = ARCamera.transform.Find("VideoBackground");
        if (videoBg == null)
        {
            Debug.LogWarning("VideoBackground not found under AR Camera – check Vuforia init.");
            yield break;
        }

        // reparent the entire hierarchy to Main Camera (preserves local offset/scale)
        videoBg.SetParent(transform, true); // true = preserve world position/rotation

        // recurse to reparent any sub-children (if needed for the future)
        ReparentChildren(videoBg);

        // match field of view to AR Camera
        mainCam.fieldOfView = ARCamera.fieldOfView;

        if (logReparent) Debug.Log("VideoBackground reparented to Main Camera: " + videoBg.name);


        // disable AR Camera camera component because not needed again
        ARCamera.enabled = false;
    }

    // recursively reparent sub-objects to maintain hierarchy (if needed)
    private void ReparentChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.SetParent(parent, true); // re-attach under new parent
            ReparentChildren(child); // recurse
        }
    }
}