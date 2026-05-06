using UnityEngine;

public class WindMill : MonoBehaviour
{
    public Vector3 rotationSpeed = new(0f, 0f, 15f);
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}
