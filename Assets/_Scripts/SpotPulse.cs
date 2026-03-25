using UnityEngine;

public class SpotPulse : MonoBehaviour
{
    private Light spotLight;

    public float angleMin = 50f;
    public float angleMax = 90f;
    public float angleSpeed = 0.9f;

    public float intensityMin = 1.5f;
    public float intensityMax = 2f;
    public float intensitySpeed = 2f;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        // Spot angle pulse (slower)
        float angle = Mathf.Lerp(angleMin, angleMax,
            (Mathf.Sin(Time.time * angleSpeed) + 1f) / 2f);

        // Intensity pulse (faster)
        float intensity = Mathf.Lerp(intensityMin, intensityMax,
            (Mathf.Sin(Time.time * intensitySpeed) + 1f) / 2f);

        spotLight.spotAngle = angle;
        spotLight.intensity = intensity;
    }
}