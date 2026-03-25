using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Crate : MonoBehaviour
{
    public GameObject breakdust;

    public float bounceForce = 3f; // tweak this

    private Rigidbody rb;
    public GameObject Pig, Boar, Golden, Alien, Cyborg, Angel;
    public PigType pigType;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        GameManager.instance.crateCount++;
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
                case PigType.Angel: prefab = Angel; break;
            }

            Instantiate(prefab, transform.position, Quaternion.identity);
            GameManager.instance.crateCount--;
            Destroy(gameObject);
            GameObject particles = Instantiate(breakdust, transform.position, Quaternion.identity);
            Destroy(particles, 1.5f); // clean up after 1 second
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
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
    }
}
