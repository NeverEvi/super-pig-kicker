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
    public bool scorePopup = false;
    private Vector3 trueScale;
    private bool firstEnable = true;

    private bool finishFade = false;

    public string startColor = "#D14A1B00";
    public string endColor = "#D14A1BFF";

    TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        trueScale = rect.localScale;
        firstEnable = false;
        StartCoroutine(FadeAlpha());
        
    }

    private void OnEnable()
    {
        if(!firstEnable) StartCoroutine(FadeAlpha());
    }

    void Update()
    {
        if (scaling) return;

        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        rect.anchoredPosition = new Vector3(startPos.x, newY, startPos.z);
        
        if(scorePopup)
        {
            float zRot = Mathf.Sin(Time.time * frequency * 3f) * 10f;

            rect.localRotation = Quaternion.Euler(0f, 0f, zRot);
        }

        if (!scorePopup && PlayerKick.instance.isKicking && finishFade)
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
        float timer = scorePopup ? 0.3f : 1f;
        while (t < timer)
        {
            t += Time.deltaTime;
            text.color = Color.Lerp(start,end, t);
            yield return null;
        }
        text.color = end;
        finishFade = true;
        StartCoroutine(Disappear(true));
    }

    IEnumerator Disappear(bool wait=false)
    {
        if (wait) 
        { 
            yield return new WaitForSeconds(!scorePopup ? 12f : 0.45f);
            
        }
        if (scaling) yield break;
        scaling = true;

        float t = 0f;
        Vector3 startScale = rect.localScale;
        if (scorePopup) scaleTime = 0.25f;
        while (t < scaleTime)
        {
            t += Time.deltaTime;
            rect.localScale = Vector3.Lerp(startScale, Vector3.zero, t / scaleTime);
            yield return null;
        }

        if (!scorePopup) Destroy(gameObject);
        else PopupReset(); 
    }
    private void PopupReset()
    {
        rect.localScale = trueScale;
        ColorUtility.TryParseHtmlString(startColor, out Color start);
        text.color = start;
        finishFade = false;
        scaling = false;
        this.gameObject.SetActive(false);
    }
        
}
