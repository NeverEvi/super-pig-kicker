using UnityEngine;

public class SpotPulse : MonoBehaviour
{
    private Light spotLight;

    public float angleMin = 50f, angleMax = 90f;
    public float angleSpeed = 0.9f;

    public float intensityMin = 1.5f, intensityMax = 2f;
    public float intensitySpeed = 2f;

    void Start() => spotLight = GetComponent<Light>();

    void Update()
    {
        float angle;
        if (spotLight.type == LightType.Spot)
        {
            angle = Mathf.Lerp(angleMin, angleMax,
                (Mathf.Sin(Time.time * angleSpeed) + 1f) / 2f);
            spotLight.spotAngle = angle;
        }

        float intensity = Mathf.Lerp(intensityMin, intensityMax,
            (Mathf.Sin(Time.time * intensitySpeed) + 1f) / 2f);
        spotLight.intensity = intensity;
    }
}