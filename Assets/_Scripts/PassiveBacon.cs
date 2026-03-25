using System.Collections;
using UnityEngine;

public class PassiveBacon : MonoBehaviour
{
    public static PassiveBacon instance;

    [Header("Bacon Settings")]
    public GameObject baconPrefab;      // assign in Inspector
    public float spawnInterval = 60f;   // start at 60 sec
    public float throwForce = 2.2f;     // force to "eject" bacon

    private float timer;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;

            // Spawn the bacon at the trough's position
            Vector3 spawnPos = transform.position + Vector3.up * 1f; // slight offset above trough
            GameObject bacon = Instantiate(baconPrefab, spawnPos, Quaternion.identity);

            // Apply a forward force relative to the trough's forward direction
            Rigidbody rb = bacon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float side = Random.Range(-0.9f, 0.9f);
                float oomph = Random.Range(-0.4f, 1f);
                Vector3 forceDir = transform.forward + transform.right * side;
                rb.AddForce(forceDir.normalized * (throwForce+oomph), ForceMode.Impulse);
            }
        }
    }
}
