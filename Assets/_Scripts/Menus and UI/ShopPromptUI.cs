using System.Collections;
using TMPro;
using UnityEngine;

public class ShopPromptUI : MonoBehaviour
{
    public GameObject shopPromptButton; // Assign your UI button in inspector
    public ShopManager shopManager;     // Reference to your shop manager script
    
    private bool shownOnce = false;
    private RectTransform buttonRect;
    private Vector3 startPos;

    [Header("Bob Settings")]
    public float amplitude = 10f;  // How far it moves up/down (pixels)
    public float frequency = 2f;   // How fast it bobs

    TextMeshProUGUI text;

    void Start()
    {
        shopPromptButton.SetActive(false);
        if (shopPromptButton != null)
        {
            buttonRect = shopPromptButton.GetComponent<RectTransform>();
            startPos = buttonRect.anchoredPosition;
        }
        text = shopPromptButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // Show when player reaches 25 bacon for the first time
        if (!shownOnce && GameManager.instance.baconCount >= 15)
        {
            shopPromptButton.SetActive(true);
            StartCoroutine(FadeAlpha());
            shownOnce = true;
        }

        // Hide when shop is opened
        if (shownOnce && shopManager.isOpen)
        {
            
            shopPromptButton.SetActive(false);
        }

        if (shopPromptButton.activeSelf && buttonRect != null)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
            buttonRect.anchoredPosition = new Vector3(startPos.x, newY, startPos.z);
        }
    }
    IEnumerator FadeAlpha()
    {
        yield return new WaitForSeconds(0.1f);
        float t = 0f;

        ColorUtility.TryParseHtmlString("#D14A1B00", out Color startColor); // transparent
        ColorUtility.TryParseHtmlString("#D14A1BFF", out Color endColor);   // fully visible
        while (t < 1f)
        {
            t += Time.deltaTime;
            text.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        text.color = endColor;

    }

}
