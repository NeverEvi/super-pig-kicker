using UnityEngine;

public class RadiusChecker : MonoBehaviour
{
    public float radius = 5f;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new (0.4f, 0.5f, 1f, 0.45f); // light blue, semi-transparent
        Gizmos.DrawSphere(transform.position, radius);
    }
}
