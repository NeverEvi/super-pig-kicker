using System.Collections;
using TMPro;
using UnityEngine;

public class Pulsate : MonoBehaviour
{
    [Header("Bob Settings")]
    public float amplitude = 10f;
    public float frequency = 2f;

    private Vector3 startPos;
    private RectTransform rect;
    private float scaleTime = 0.7f;
    private bool scaling = false;

    private bool finishFade = false;
    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        StartCoroutine(FadeAlpha());
        
    }

    void Update()
    {
        if (scaling) return;

        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        rect.anchoredPosition = new Vector3(startPos.x, newY, startPos.z);

        if (PlayerKick.instance.isKicking && finishFade)
        {
            StopAllCoroutines();
            StartCoroutine(Disappear());
        }
    }

    IEnumerator FadeAlpha()
    {
        yield return new WaitForSeconds(0.5f);
        float t = 0f;

        ColorUtility.TryParseHtmlString("#D14A1B00", out Color startColor); // transparent
        ColorUtility.TryParseHtmlString("#D14A1BFF", out Color endColor);   // fully visible
        while (t < 1f)
        {
            t += Time.deltaTime;
            text.color = Color.Lerp(startColor,endColor, t);
            yield return null;
        }
        text.color = endColor;
        finishFade = true;
        StartCoroutine(Disappear(true));
    }

    IEnumerator Disappear(bool wait=false)
    {
        if (wait) 
        { 
            yield return new WaitForSeconds(12f);
            
        }
        if (scaling) yield break;
        scaling = true;

        float t = 0f;
        Vector3 startScale = rect.localScale;

        while (t < scaleTime)
        {
            t += Time.deltaTime;
            rect.localScale = Vector3.Lerp(startScale, Vector3.zero, t / scaleTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
