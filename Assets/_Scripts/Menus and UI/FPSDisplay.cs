using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;   // Assign in inspector
    public float updateInterval = 0.5f; // seconds between FPS updates

    private float timer = 0f;
    private int frames = 0;

    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            float fps = frames / timer;
            if (fpsText != null)
                fpsText.text = $"FPS: {fps:F1}";

            frames = 0;
            timer = 0f;
        }
    }
}
