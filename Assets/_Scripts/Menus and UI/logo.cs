using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Logo : MonoBehaviour
{
    private Vector3 originalScale;
    private Image image;
    private SpriteRenderer sprite;
    private Coroutine routine;

    void Awake()
    {
        originalScale = transform.localScale;
        image = GetComponent<Image>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        transform.localScale = originalScale;
        SetAlpha(1f);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeAndShrink());
    }

    void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    IEnumerator FadeAndShrink()
    {
        yield return new WaitForSeconds(0.2f);

        float duration = 1.3f;
        float time = 0f;
        Vector3 targetScale = originalScale * 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            SetAlpha(Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        transform.localScale = targetScale;
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
        else if (sprite != null)
        {
            Color c = sprite.color;
            c.a = alpha;
            sprite.color = c;
        }
    }
}