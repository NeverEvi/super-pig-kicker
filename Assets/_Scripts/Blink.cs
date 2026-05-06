using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour
{
    public GameObject eye1;
    public GameObject eye2;

    public float blinkScale = 0.001f;
    public float blinkSpeed = 0.05f; // how fast eyes close/open
    public float minDelay = 5f;
    public float maxDelay = 10f;

    private Vector3 eye1OriginalScale;
    private Vector3 eye2OriginalScale;

    void Start()
    {
        eye1OriginalScale = eye1.transform.localScale;
        if (eye2 != null)
            eye2OriginalScale = eye2.transform.localScale;

        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Wait random time
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Close eyes
            yield return StartCoroutine(ScaleEyes(blinkScale));

            // Open eyes
            yield return StartCoroutine(ScaleEyes(eye1OriginalScale.y));
        }
    }

    IEnumerator ScaleEyes(float targetY)
    {
        float t = 0f;

        float startY1 = eye1.transform.localScale.y;
        float startY2 = 0;
        if (eye2 != null) startY2  = eye2.transform.localScale.y;

        while (t < 1f)
        {
            t += Time.deltaTime / blinkSpeed;

            float newY1 = Mathf.Lerp(startY1, targetY, t);
            float newY2 = Mathf.Lerp(startY2, targetY, t);

            eye1.transform.localScale = new Vector3(eye1OriginalScale.x, newY1, eye1OriginalScale.z);
            if(eye2 != null) eye2.transform.localScale = new Vector3(eye2OriginalScale.x, newY2, eye2OriginalScale.z);

            yield return null;
        }
    }
}