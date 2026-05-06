using TMPro;
using UnityEngine;

public class Bacon : MonoBehaviour
{
    public Vector3 rotationSpeed = new(0f, 45f, 0f); // degrees per second
    public float floatAmplitude = 0.25f; // how high it bobs
    public float floatFrequency = 0.5f; // how fast it bobs
    public GameObject model;
    public AudioClip pickupSound;
    public AudioClip suckSound;
    public float pickupVolume = 1f;
    private Vector3 startLocalPos;
    public int baconValue = 1;
    public TextMeshPro display;

    void Start()
    {
        if (model != null)
            startLocalPos = model.transform.localPosition;
        display.text = baconValue.ToString();
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
        if ((other.CompareTag("Player") && !GameManager.instance.immune) || other.CompareTag("SuckerPoint"))
        {
        
            GameManager.instance.AddBacon(baconValue);
            if (pickupSound != null)
                if (other.CompareTag("SuckerPoint")) 
                {
                    //AudioSource.PlayClipAtPoint(suckSound, transform.position, pickupVolume);
                    AudioHelper.PlayClipAtPosition(suckSound, transform.position, pickupVolume, Random.Range(0.95f, 1.1f));
                }
                else
                { 
                    //AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
                    AudioHelper.PlayClipAtPosition(pickupSound, transform.position, pickupVolume, Random.Range(0.9f, 1.1f));
                }
            
            Destroy(transform.parent.gameObject); // destroys the Rigidbody parent
        
        }
        
    }
}
