using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;      // how fast it rises
    public float duration = 1f;        // how long before it disappears
    public Color startColor = Color.white;
    public Color endColor = new(1, 1, 1, 0); // fades to transparent

    private TextMeshPro tmp;
    private float timer = 0f;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        tmp.color = startColor;
    }

    void Update()
    {
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
        timer += Time.deltaTime;

        // fade
        float t = timer / duration;
        if (tmp != null)
            tmp.color = Color.Lerp(startColor, endColor, t);

        if (timer >= duration)
            Destroy(gameObject);
    }

    public void SetText(string text)
    {
        if (tmp != null)
            tmp.text = text;
    }
}