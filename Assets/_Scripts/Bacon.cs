using UnityEngine;

public class Bacon : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0f, 45f, 0f); // degrees per second
    public float floatAmplitude = 0.25f; // how high it bobs
    public float floatFrequency = 0.5f; // how fast it bobs
    public GameObject model; // assign the mesh/model child here
    public AudioClip pickupSound; // assign your bacon pickup sound here
    public float pickupVolume = 1f;
    private Vector3 startLocalPos;
    public int baconValue = 1;

    void Start()
    {
        if (model != null)
            startLocalPos = model.transform.localPosition;
    }

    void Update()
    {
        // rotate the parent (Rigidbody) freely
        model.transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);

        // bob the visual model only
        if (model != null)
        {
            Vector3 pos = startLocalPos;
            pos.y += Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2) * floatAmplitude;
            model.transform.localPosition = pos;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.AddBacon(baconValue);
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
            Destroy(transform.parent.gameObject); // destroys the Rigidbody parent
        }
    }
}
