using UnityEngine;

public class AndroidLightSensor : MonoBehaviour
{
    private AndroidJavaObject sensorManager;
    private AndroidJavaObject lightSensor;
    private AndroidJavaObject sensorEventListener;
    private float lux;

    public float Lux => lux;

    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                sensorManager = activity.Call<AndroidJavaObject>("getSystemService", "sensor");
                lightSensor = sensorManager.Call<AndroidJavaObject>("getDefaultSensor", 5); // TYPE_LIGHT = 5

                sensorEventListener = new LightSensorListener(this);
                sensorManager.Call("registerListener", sensorEventListener, lightSensor, 3);
        #endif
    }

    void OnDestroy()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                if (sensorManager != null && sensorEventListener != null)
                    sensorManager.Call("unregisterListener", sensorEventListener);
        #endif
    }

    private class LightSensorListener : AndroidJavaProxy
    {
        private AndroidLightSensor parent;
        public LightSensorListener(AndroidLightSensor parent) 
            : base("android.hardware.SensorEventListener") 
        { 
            this.parent = parent; 
        }

        void onSensorChanged(AndroidJavaObject sensorEvent)
        {
            float[] values = sensorEvent.Get<float[]>("values");
            parent.lux = values[0];
        }

        void onAccuracyChanged(AndroidJavaObject sensor, int accuracy) { }
    }
}