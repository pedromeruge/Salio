using System;
using UnityEngine;
using Vuforia;

public class AdaptiveFogController : MonoBehaviour
{
    public float brightnessThreshold = 0.4f;
    public float fogDensityDark = 0.15f;
    public float fogDensityBright = 0.0f;
    public float smoothSpeed = 2.0f;

    private float currentFogDensity = 0f;
    private bool vuforiaInitialized = false;
    private bool cameraAccessEnabled = false;

    private void Start()
    {
        // Subscribe to Vuforia initialization event
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaInitialized -= OnVuforiaInitialized;
        }
    }

    private void OnVuforiaInitialized(VuforiaInitError initError)
    {
        if (initError == VuforiaInitError.NONE)
        {
            vuforiaInitialized = true;
            Debug.Log("Vuforia initialized successfully for AdaptiveFogController");
            
            // Try to enable camera access in the next frame
            StartCoroutine(EnableCameraAccessDelayed());
        }
        else
        {
            Debug.LogError($"Vuforia initialization failed: {initError}");
        }
    }

    private System.Collections.IEnumerator EnableCameraAccessDelayed()
    {
        // Wait for camera to be fully ready
        yield return new WaitForSeconds(0.5f);

        var cameraDevice = VuforiaBehaviour.Instance?.CameraDevice;
        if (cameraDevice != null)
        {
            if (cameraDevice.SetFrameFormat(PixelFormat.RGB888, true))
            {
                cameraAccessEnabled = true;
                Debug.Log("Camera frame format set successfully");
            }
            else
            {
                Debug.LogWarning("Failed to set camera frame format - trying GRAYSCALE");
                // Fallback to grayscale if RGB888 fails
                if (cameraDevice.SetFrameFormat(PixelFormat.GRAYSCALE, true))
                {
                    cameraAccessEnabled = true;
                    Debug.Log("Camera frame format set to GRAYSCALE");
                }
            }
        }
    }

    void Update()
    {
        if (!vuforiaInitialized || !cameraAccessEnabled)
            return;

        if (VuforiaBehaviour.Instance == null || VuforiaBehaviour.Instance.CameraDevice == null)
            return;

        // Try RGB888 first, fallback to GRAYSCALE
        Vuforia.Image image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PixelFormat.RGB888);
        if (image == null)
        {
            image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PixelFormat.GRAYSCALE);
        }
        
        if (image == null) return;

        float brightness = CalculateAverageBrightness(image);
        AdjustFog(brightness);
    }

    float CalculateAverageBrightness(Vuforia.Image image)
    {
        byte[] pixels = image.Pixels;
        if (pixels == null || pixels.Length == 0) return 1f;

        int step = 10; 
        long total = 0;
        int count = 0;
        int stride = image.PixelFormat == PixelFormat.RGB888 ? 3 : 1;

        for (int i = 0; i < pixels.Length; i += stride * step)
        {
            if (stride == 3) // RGB888
            {
                byte r = pixels[i];
                byte g = pixels[i + 1];
                byte b = pixels[i + 2];
                total += (r + g + b) / 3;
            }
            else // GRAYSCALE
            {
                total += pixels[i];
            }
            count++;
        }

        float avg = (float)total / (count * 255f);
        return avg;
    }

    void AdjustFog(float brightness)
    {
        float targetFog = brightness < brightnessThreshold ? fogDensityDark : fogDensityBright;
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFog, Time.deltaTime * smoothSpeed);

        RenderSettings.fog = currentFogDensity > 0.001f;
        RenderSettings.fogDensity = currentFogDensity;
    }
}