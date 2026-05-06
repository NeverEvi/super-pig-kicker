using UnityEngine;

public class PlayerSuction : MonoBehaviour
{
    [Header("Suction")]
    public float pullStrength = 0.03f;
    public float maxPullSpeed = 0.5f;

    [Header("Bacon")]
    public string baconTag = "Bacon";

    void FixedUpdate()
    {
        GameObject[] baconObjects = GameObject.FindGameObjectsWithTag(baconTag);

        foreach (GameObject bacon in baconObjects)
        {
            if (bacon == null) continue;
            if (!bacon.TryGetComponent<AntiSuckTimer>(out AntiSuckTimer timer)) continue;
            if (timer.age < 10f) continue;
            if (!bacon.TryGetComponent<Rigidbody>(out Rigidbody rb)) continue;
        

            Vector3 dir = transform.position - rb.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f) continue;

            dir.Normalize();

            rb.linearVelocity += dir * pullStrength;

            if (rb.linearVelocity.magnitude > maxPullSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxPullSpeed;
            }
        }
    }
}