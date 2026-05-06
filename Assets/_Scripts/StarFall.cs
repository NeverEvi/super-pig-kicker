using UnityEngine;

public class StarFall : MonoBehaviour
{
    public static StarFall instance;
    public float timer = 0f;
    private float spawnTime = 30f;
    public GameObject starPrefab;
    private void Awake()
    {
        instance = this;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnTime) 
        {
            Vector3 spawnpos = new(Random.Range(-2f, 6f), Random.Range(4f, 5.5f), Random.Range(-2f, 4f));
            GameObject star = Instantiate(starPrefab, spawnpos, Quaternion.identity);
            timer = 0f;
            spawnTime = DayCycle.instance.isDay ? Random.Range(15f, 40f): Random.Range(12f,35f);
            if (star.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Slight sideways nudge
                Vector3 sideways = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                );

                rb.AddForce(sideways * Random.Range(0.5f, 2f), ForceMode.Impulse);

                // Add spin
                Vector3 spin = new Vector3(
                    0f,
                    Random.Range(-2f, 2f),
                    Random.Range(-5f, 5f)
                );

                rb.AddTorque(spin, ForceMode.Impulse);
            }
        }
    }
}
