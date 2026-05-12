using System.Collections;
using TMPro;
using UnityEngine;

public class Pulsate : MonoBehaviour
{
    [Header("Bob Settings")]
    public float amplitude = 10f;
    public float frequency = 2f;

    [Header("Color Settings")]
    public string startColor = "#D14A1B00";
    public string endColor = "#D14A1BFF";

    [Header("Popup Settings")]
    public bool scorePopup = false;
    public float spinSpeedMultiplier = 3f;
    public float spin = 10f;
    private float wobbleTime = 0f;

    private Vector3 startPos;
    private RectTransform rect;

    private readonly float fadeOutTime = 0.7f;
    private bool fadingOUT = false;
    private bool finishFadeIN = false;

    private bool firstEnable = true;

    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        firstEnable = false;
        StartCoroutine(FadeAlpha());
        
    }

    private void OnEnable()
    {
        if(!firstEnable) StartCoroutine(FadeAlpha());
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        rect.anchoredPosition = new Vector3(startPos.x, newY, startPos.z);

        if (scorePopup && finishFadeIN)
        {
            wobbleTime += Time.deltaTime;

            float zRot = Mathf.Sin(wobbleTime * frequency * spinSpeedMultiplier) * spin;
            rect.localRotation = Quaternion.Euler(0f, 0f, zRot);
        }

        if (!scorePopup && PlayerKick.instance.isKicking && finishFadeIN && !fadingOUT)
        {
            StopAllCoroutines();
            StartCoroutine(Disappear());
        }
    }

    IEnumerator FadeAlpha()
    {
        if(!scorePopup) yield return new WaitForSeconds(0.5f);

        float t = 0f;

        ColorUtility.TryParseHtmlString(startColor, out Color start); // transparent
        ColorUtility.TryParseHtmlString(endColor, out Color end);   // fully visible //FFEE00

        float timer = scorePopup ? 0.25f : 1f;

        while (t < timer)
        {
            t += Time.deltaTime;
            float progress = t / timer;
            
            text.color = Color.Lerp(start, end, progress);

            if (scorePopup && spin>10)
            {
                float spinRot = Mathf.Lerp(360f, -spin, progress);
                rect.localRotation = Quaternion.Euler(0f, 0f, spinRot);
                
            }
            wobbleTime = 0f;
            yield return null;
        }
        text.color = end;
        finishFadeIN = true;
        StartCoroutine(Disappear(true));
    }

    IEnumerator Disappear(bool wait = false)
    {
        if (wait)
        {
            yield return new WaitForSeconds(!scorePopup ? 12f : 0.5f);
        }

        if (fadingOUT) yield break;
        fadingOUT = true;

        float t = 0f;
        float fadeTime = scorePopup ? 0.25f : fadeOutTime;

        Color start = text.color;
        Color end = start;
        end.a = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            text.color = Color.Lerp(start, end, t / fadeTime);
            yield return null;
        }

        text.color = end;

        if (!scorePopup) Destroy(gameObject);
        else PopupReset();
    }
    private void PopupReset()
    {
        ColorUtility.TryParseHtmlString(startColor, out Color start);
        text.color = start;

        wobbleTime = 0f;
        finishFadeIN = false;
        fadingOUT = false;
        this.gameObject.SetActive(false);
    }
        
}
