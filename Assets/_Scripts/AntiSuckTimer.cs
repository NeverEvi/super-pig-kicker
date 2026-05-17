using UnityEngine;

public class AntiSuckTimer : MonoBehaviour
{
    public float age = 0f;

    void Update()
    {
        if (age < 10f) age += Time.deltaTime;
    }
}
