using UnityEngine;

public class CarrotPatch : MonoBehaviour
{
    public static CarrotPatch instance;

    [Header("Carrot Settings")]
    public GameObject carrotPrefab;   // prefab to spawn
    public float spawnInterval = 60f; // seconds between spawns
    public GameObject currentCarrot; // reference to spawned carrot
    private float timer;


    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (currentCarrot == null)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnCarrot(); timer = 0f;
            }
        }

    }

    private void SpawnCarrot()
    {
        if (carrotPrefab == null) return;
        Vector3 spawnpos = transform.position;
        spawnpos.y += 0.1f;
        currentCarrot = Instantiate(carrotPrefab, spawnpos, Quaternion.Euler(90f, 0f, 0f));
        Rigidbody rb = currentCarrot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; rb.useGravity = false;
        }
        Debug.Log("Carrot Spawned");
        //currentCarrot.AddComponent<Carrot>();
    }

    public void ClearCarrot()
    {
        currentCarrot = null;
    }
}