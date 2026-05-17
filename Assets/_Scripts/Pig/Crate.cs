using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.ParticleSystem;

public class Crate : MonoBehaviour
{
    public GameObject breakdust;

    public float bounceForce = 3f; // tweak this

    private Rigidbody rb;
    public GameObject Pig, Boar, Golden, Alien, Cyborg, Cyboarg, Angel;
    public PigType pigType;

    [Header("Shake")]
    public float minShakeDelay = 5f;
    public float maxShakeDelay = 9f;

    public float thumpStrength = 0.08f;
    public float thumpDuration = 0.08f;
    public float thumpUpward = 0.03f;
    public float pauseBetweenThumps = 0.12f;
    public AudioClip hitClip;
    public AudioClip groundClip;
    private AudioSource audioSource;
    private bool hitGround = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        GameManager.instance.crateCount++;
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(ThumpLoop());
    }
    public void Kick()
    {
        int pigs = GameManager.instance.pigsCount;
        int maxpigs = GameManager.instance.pigsMax;
        if (pigs < maxpigs) 
        {
            GameObject prefab = null;
            switch (pigType)
            {
                case PigType.Pig: prefab = Pig; break;
                case PigType.Boar: prefab = Boar; break;
                case PigType.Golden: prefab = Golden; break;
                case PigType.Alien: prefab = Alien; break;
                case PigType.Cyborg: prefab = Cyborg; break;
                case PigType.Cyboarg: prefab = Cyboarg; break;
                case PigType.Angel: prefab = Angel; break;
            }

            Instantiate(prefab, transform.position, Quaternion.identity);
            GameManager.instance.crateCount--;
            AudioHelper.PlayClipAtPosition(
            hitClip,
            transform.position,
            1f,
            Random.Range(0.9f, 1.1f)
        );
            Destroy(gameObject);
            Instantiate(
                breakdust, 
                transform.position- new Vector3(0f,0.5f,0f),
                Quaternion.Euler(90f, 0f, 0f)
                );
        }
    }

    private IEnumerator ThumpLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minShakeDelay, maxShakeDelay));

            yield return StartCoroutine(DoubleThump());
        }
    }
    private IEnumerator DoubleThump()
    {
        Vector3 originalPos = transform.localPosition;

        // Pick a random horizontal direction
        Vector3 dir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // First thump
        yield return StartCoroutine(DoThump(originalPos, dir));

        yield return new WaitForSeconds(pauseBetweenThumps);

        float noSecondThumpChance = 0.15f;
        if (Random.Range(0f, 1f) > noSecondThumpChance)
        {
            // Second thump (slightly stronger feels good)
            yield return StartCoroutine(DoThump(originalPos, dir * 1.2f));
        }
        transform.localPosition = originalPos;
    }
    private IEnumerator DoThump(Vector3 basePos, Vector3 dir)
    {
        float timer = 0f;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(hitClip);
        
        while (timer < thumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / thumpDuration;
            
            // Push out, then snap back (ease)
            float strength = Mathf.Sin(t * Mathf.PI);

            Vector3 horizontalOffset = dir * thumpStrength * strength;
            Vector3 verticalOffset = Vector3.up * thumpUpward * strength;

            transform.localPosition = basePos + horizontalOffset + verticalOffset;

            yield return null;
        }
        float breakout = 0.01f;
        float breakoutRoll = Random.Range(0f, 1f);
        if (breakoutRoll < breakout)
        {
            Debug.Log("BREAKOUT!");
            Kick();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !hitGround)
        {
            audioSource.PlayOneShot(groundClip);
            hitGround = true;
        }
    }

    public CrateSaveData GetSaveData()
    {
        return new CrateSaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,

            rotX = transform.rotation.x,
            rotY = transform.rotation.y,
            rotZ = transform.rotation.z,
            rotW = transform.rotation.w,

            pigType = (int)pigType
        };
    }

    public void LoadFromSaveData(CrateSaveData data)
    {
        pigType = (PigType)data.pigType;
        transform.SetPositionAndRotation(
        new Vector3(data.posX, data.posY, data.posZ),
        new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW));
    }
}
