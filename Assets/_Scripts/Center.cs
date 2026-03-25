using UnityEngine;

public class Center : MonoBehaviour
{
    public static Center instance;
    public float radius = 5f;
    void Awake() => instance = this;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.5f, 1f, 0.45f); // light blue, semi-transparent
        Gizmos.DrawSphere(transform.position, radius);
    }
}
