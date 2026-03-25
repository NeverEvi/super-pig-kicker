using UnityEngine;

public class HaloSpin : MonoBehaviour
{
    public float rotationSpeed = 90f; // degrees per second

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
