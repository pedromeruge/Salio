using UnityEngine;
using Vuforia;

public class AdaptiveFogController : MonoBehaviour
{
    public float brightnessThreshold = 0.4f;
    public float fogDensityDark = 0.05f;
    public float fogDensityBright = 0.0f;
    public float smoothSpeed = 2.0f;

    private float currentFogDensity = 0f;
    private bool vuforiaInitialized = false;


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
        }
        else
        {
            Debug.LogError($"Vuforia initialization failed: {initError}");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!vuforiaInitialized)
            return;

        // Additional safety check
        if (VuforiaBehaviour.Instance == null || VuforiaBehaviour.Instance.CameraDevice == null)
            return;

        Vuforia.Image image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PixelFormat.RGB888);
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

        for (int i = 0; i < pixels.Length; i += 3 * step)
        {
            byte r = pixels[i];
            byte g = pixels[i + 1];
            byte b = pixels[i + 2];
            total += (r + g + b) / 3;
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
