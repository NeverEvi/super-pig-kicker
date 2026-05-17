using System.Collections;
using UnityEngine;

public class LegScale : MonoBehaviour
{
    public float scaleDuration = 2f;

    void Start()
    {
        StartCoroutine(YScaleLegs());
    }

    IEnumerator YScaleLegs()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(startScale.x, 1f, startScale.z);

        float timer = 0f;
        yield return new WaitForSeconds(0.2f);
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;

            float t = timer / scaleDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        transform.localScale = targetScale;
    }
}