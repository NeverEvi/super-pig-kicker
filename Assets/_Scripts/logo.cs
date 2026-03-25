using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class logo : MonoBehaviour
{
    private Vector3 originalScale;
    private Image image; // if this is a UI logo (Image)
    private SpriteRenderer sprite; // if it’s a 2D sprite

    void Start()
    {
        originalScale = transform.localScale;
        image = GetComponent<Image>();
        sprite = GetComponent<SpriteRenderer>();

        StartCoroutine(FadeAndShrink());
    }

    IEnumerator FadeAndShrink()
    {
        // wait 0.2 second first
        yield return new WaitForSeconds(0.2f);

        float duration = 1.3f;
        float time = 0f;

        Vector3 targetScale = originalScale * 0.8f;

        // get starting color
        Color startColor = Color.white;
        if (image != null) startColor = image.color;
        else if (sprite != null) startColor = sprite.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // scale down
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            // fade out
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            if (image != null) image.color = newColor;
            else if (sprite != null) sprite.color = newColor;

            yield return null;
        }

        // make sure it's fully applied at the end
        transform.localScale = targetScale;
        if (image != null) image.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        else if (sprite != null) sprite.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
