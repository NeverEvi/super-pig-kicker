using System.Collections;
using UnityEngine;

public class PassiveBacon : MonoBehaviour
{
    public static PassiveBacon instance;

    [Header("Bacon Settings")]
    public GameObject baconPrefab;      // assign in Inspector
    public float spawnInterval = 60f;   // start at 60 sec
    public float throwForce = 2.2f;     // force to "eject" bacon
    public float upForce = 1.5f;
    public float sideRange = 1.25f;
    public float spawnOffset = 0.8f;

    private float timer;
    public bool running = false;
    private void Awake() => instance = this;

    void Update()
    {
        if (!running) return; 
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            // Spawn the bacon at the trough's position
            Vector3 spawnPos = transform.position + Vector3.up * spawnOffset; // slight offset above trough
            GameObject bacon = Instantiate(baconPrefab, spawnPos, Quaternion.identity);

            // Apply a forward force relative to the trough's forward direction
            if (bacon.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                float side = Random.Range(-sideRange, sideRange);
                float oomph = Random.Range(0.3f, 0.5f);
                Vector3 forceDir = 
                    Vector3.up * upForce + 
                    transform.forward * 1.1f + 
                    transform.right * side;
                rb.AddForce(forceDir.normalized * throwForce * oomph, ForceMode.Impulse);
            }
        }
    }
}
