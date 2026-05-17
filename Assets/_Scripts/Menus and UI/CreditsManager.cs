using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    public static CreditsManager instance;
    public GameObject EndCredits;
    public RectTransform CreditsTransform;

    void Awake() => instance = this;

    public void OpenCredits(bool end = false)
    {
        EndCredits.SetActive(true);
        StartCoroutine(RollCredits(end));
    }
    public void CloseCredits()
    {
        if (EndCredits != null)
        {
            StopAllCoroutines();
            if (EndCredits.TryGetComponent(out CanvasGroup canvas)) canvas.alpha = 0f;
            RectTransform rect = CreditsTransform;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
            EndCredits.SetActive(false);
        }
    }


    public IEnumerator RollCredits(bool end = false)
    {
        if (!EndCredits.TryGetComponent(out CanvasGroup canvas)) yield break;
        if(EndCredits.TryGetComponent(out Button button))
        {
            button.interactable = !end;
        }

        if(end) ShopManager.instance.playing = false;
        RectTransform rect = CreditsTransform;

        canvas.alpha = 0f;

        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            canvas.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        canvas.alpha = 1f;

        yield return new WaitForSecondsRealtime(0.5f);

        // Scroll until a target Y position
        float speed = 35f;
        float endY = 3000f; // tweak this to wherever the credits should stop

        while (rect.anchoredPosition.y < endY)
        {
            rect.anchoredPosition += Vector2.up * (speed * Time.unscaledDeltaTime);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Fade out
        t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            canvas.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        CloseCredits();
    }
}
