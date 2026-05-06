using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    public Image image;
    public TextMeshProUGUI text;

    private readonly Queue<PopupData> popupQueue = new();
    private bool isPlaying = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        image.transform.localScale = Vector3.zero;
    }

    public void DisplayPopup(Sprite display, string textString)
    {
        popupQueue.Enqueue(new PopupData(display, textString));

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (popupQueue.Count > 0)
        {
            PopupData popup = popupQueue.Dequeue();

            image.sprite = popup.sprite;
            text.text = popup.message;

            yield return StartCoroutine(AnimateDisplay());
        }

        isPlaying = false;
    }

    private IEnumerator AnimateDisplay()
    {
        Transform target = image.transform;

        target.localScale = Vector3.zero;

        // Scale up to 1 over 1 second
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float t = timer / 1f;
            target.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        target.localScale = Vector3.one;

        // Bounce between 0.9 and 1.1 for 2 seconds
        timer = 0f;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            float bounce = Mathf.Sin(timer * Mathf.PI * 4f);
            float scale = 1f + (bounce * 0.1f);
            target.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        target.localScale = Vector3.one;

        // Scale down to 0 over 1 second
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float t = timer / 1f;
            target.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        target.localScale = Vector3.zero;
    }

    private class PopupData
    {
        public Sprite sprite;
        public string message;

        public PopupData(Sprite sprite, string message)
        {
            this.sprite = sprite;
            this.message = message;
        }
    }
}