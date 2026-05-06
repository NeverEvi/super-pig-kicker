using UnityEngine;

public class AntiSuckTimer : MonoBehaviour
{
    public float age = 0f;

    // Update is called once per frame
    void Update()
    {
        if (age < 10f)
        {
            age += Time.deltaTime;
        }
    }
}
