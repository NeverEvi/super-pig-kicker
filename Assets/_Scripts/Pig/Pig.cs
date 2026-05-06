using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Pig : MonoBehaviour
{
    public Transform spawnPoint;

    public PigCombat combat;

    public GameObject pigdust;

    [Header("Wander")]

    public float wanderRadius = 2f; // max random offset for wandering
    public float wanderChangeInterval = 2f; // seconds between direction changes
    private Vector3 wanderTarget;
    private Vector3 wanderCenter = new (2.02f, 0.43f, 1.54f);

    [Header("Kicking")]
    private Rigidbody rb;
    public float kickForce = 5f; // how strongly the pig flies back
    
    public bool isUpright = true;
    private float uprightDelay = 1.7f; // seconds to stand back up
    private Coroutine standUprightRoutine;

    [Header("Stats")]
    public int hp = 30, mhp = 30;
    public int baconPerKick = 1;
    public float wanderSpeed = 1.5f; // movement speed while wandering
    public PigType pigType;

    [Header("Healing")]
    public string carrotTag = "Carrot"; 
    public float healCheckInterval = 2f; // seconds between scans
    public float eatDistance = 0.3f; // how close to "eat" carrot
    private Transform carrotTarget;

    [Header("Legs")]
    public GameObject frontLeftLeg;
    public GameObject frontRightLeg;
    public GameObject backLeftLeg;
    public GameObject backRightLeg;

    public float legSwingAmplitude = 20f; // max rotation angle
    public float legSwingSpeed = 5f;      // how fast legs swing
    private float legTimer = 0f;

    public bool animateLegs = true;
    public bool beenKicked = false;

    [Header("UI")]
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new (0, 1f, 0); // above pig's head
    
    [Header("Audio")]
    public AudioClip oinkSound; // assign a pig oink sound in inspector
    public AudioClip eatSound;
    private AudioSource audioSource;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        // add or get an AudioSource

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(WanderRoutine());
        StartCoroutine(HealCheckRoutine());
        
        GameManager.instance.pigsCount++;
        GameManager.instance.UpdatePigCount();
        
    }

    void Update()
    {
        if (isUpright)
        {
            if (pigType != PigType.Devil) AnimateLegs(true);
            HandleMovement();
        }
    }

    public void Kick(int damage, Transform kicker = null)
    {

        // play the pig sound
        if (audioSource != null && oinkSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(oinkSound);
        }
        Damage.instance.BaconDamage(damage, Vector3.zero, this);

        // apply knockback force
        if (rb != null && kicker != null)
        {
            Vector3 forceDir = (transform.position - kicker.position).normalized + Vector3.up * 0.3f;
            rb.AddForce(forceDir * kickForce, ForceMode.Impulse);
            float spinStrength = 1f;
            rb.AddTorque(Vector3.forward * spinStrength, ForceMode.Impulse);
        }

        hp -= damage;
        if (!beenKicked)
        {
            beenKicked = true;
            GameManager.instance.UpdateKickedPigs();
        }
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            isUpright = false;
            // start upright coroutine
            StartStandUpright();
        }
    }
    public void Die()
    {
        AudioHelper.PlayClipAtPosition(
            oinkSound,
            transform.position,
            1f,
            Random.Range(1.35f, 1.48f)
        );

        if (pigdust != null)
            Instantiate(pigdust, transform.position, Quaternion.identity);
        
        GameManager.instance.pigsCount--;
        GameManager.instance.UpdatePigCount();
        
        if (pigType == PigType.Devil)
        { 
            DayCycle.instance.bossFight = false;
            combat.BossUI.SetActive(false);
            Summoning.instance.Endgame();
        }
        if(pigType == PigType.Angel)
        {
            GameManager.instance.angelKills++;
            Button devilButton = ShopManager.instance.DevilButton.GetComponent<Button>();

            if (GameManager.instance.angelKills >= 3)
            {
                if(!devilButton.interactable)
                {
                    ShopManager.instance.Unlock(ShopManager.instance.DevilButton, "Devil Pig", true);
                    ShopManager.instance.DevilCostText.text = "Devil Pig: 3000 bacon";
                }
            }
            else
            {
                ShopManager.instance.Unlock(ShopManager.instance.DevilButton, "", false, false);
                devilButton.interactable = false;
                ShopManager.instance.DevilCostText.text = "Devil Pig: (" + GameManager.instance.angelKills + "/3 angels killed)";
            }
        }
        if (pigType != PigType.Devil)
            Destroy(gameObject);
    }
    void StartStandUpright()
    {
        if (standUprightRoutine != null)
        {
            StopCoroutine(standUprightRoutine);
        }

        standUprightRoutine = StartCoroutine(StandUpright());
    }
    public IEnumerator StandUpright()
    {
        yield return new WaitForSeconds(uprightDelay);
        
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            Quaternion uprightRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            float timer = 0f;
            float duration = 0.8f;

            while (timer < duration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, uprightRotation, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.rotation = uprightRotation;
        }

        isUpright = true; // pig can now wander
        standUprightRoutine = null;
    }
    private IEnumerator HealCheckRoutine()
    {
        while (true)
        {
            if (hp < mhp && carrotTarget == null)
            {
                GameObject[] carrots = GameObject.FindGameObjectsWithTag(carrotTag);
                float closestDist = Mathf.Infinity;
                GameObject closest = null;

                foreach (GameObject c in carrots)
                {
                    float dist = Vector3.Distance(transform.position, c.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = c;
                    }
                }

                if (closest != null)
                {
                    carrotTarget = closest.transform;
                }
            }
            yield return new WaitForSeconds(healCheckInterval);
        }
    }
    private float balanceCheckTime = 0f;
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (isUpright)
            {
                Vector3 randomOffset = new(
                    Random.Range(-wanderRadius, wanderRadius),
                    0f,
                    Random.Range(-wanderRadius, wanderRadius)
                );
                if(beenKicked 
                    && pigType!=PigType.Boar 
                    && pigType != PigType.Cyboarg 
                    && pigType != PigType.Devil) 
                {
                    Vector3 awayFromPlayer = (transform.position - player.transform.position).normalized;

                    // VERY slight influence (tweak this)
                    float avoidStrength = 0.3f;

                    randomOffset += awayFromPlayer * wanderRadius * avoidStrength;
                }
                wanderTarget = wanderCenter + randomOffset;
                balanceCheckTime += Time.deltaTime;
                float balanceCheckThreshold = 5f;
                if(balanceCheckTime > balanceCheckThreshold)
                {
                    if(transform.position.x >= 60 || transform.position.x <= -60 || transform.position.z >= 70 || transform.position.z <= -70)
                    {
                        StartStandUpright();
                    }

                }
            }

            yield return new WaitForSeconds(wanderChangeInterval);
        }
    }

    void HandleMovement()
    {
        if (combat && combat.charging) return;
        if (carrotTarget != null)
        {
            // Move toward carrot
            Vector3 direction = (carrotTarget.position - transform.position);
            direction.y = 0f;

            if (direction.magnitude > eatDistance)
            {
                Vector3 dir = direction.normalized;
                float move = wanderSpeed * Time.deltaTime;
                rb.MovePosition(transform.position + dir * move);
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
            }
            else
            {
                // Eat carrot
                if (CarrotPatches.instance.goldPatch.currentCarrot == carrotTarget.gameObject)
                { hp += 10; ShowFloatingText(10); }

                else { hp += 2; ShowFloatingText(2); }

                Destroy(carrotTarget.gameObject);
                carrotTarget = null;
                if (eatSound != null)
                    audioSource.PlayOneShot(eatSound);
            }
        }
        else
        {
            // normal wandering
            Vector3 direction = (wanderTarget - transform.position);
            direction.y = 0f;
            if (direction.magnitude > 0.1f)
            {
                float move = wanderSpeed * Time.deltaTime;
                rb.MovePosition(transform.position + direction.normalized * move);
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
            }
        }
    }
    void AnimateLegs(bool isWalking)
    {
        if(!isWalking)
        {
            if (frontLeftLeg) frontLeftLeg.transform.localEulerAngles = Vector3.zero;
            if (frontRightLeg) frontRightLeg.transform.localEulerAngles = Vector3.zero;
            if (backLeftLeg) backLeftLeg.transform.localEulerAngles = Vector3.zero;
            if (backRightLeg) backRightLeg.transform.localEulerAngles = Vector3.zero;
            return;
        }
        legTimer += Time.deltaTime * legSwingSpeed;
        float swing = Mathf.Sin(legTimer) * legSwingAmplitude;

        Vector3 swingForward = new(swing, 0, 0);
        Vector3 swingBack = new(-swing, 0, 0);
        if (frontLeftLeg) frontLeftLeg.transform.localEulerAngles = swingForward; 
        if (backRightLeg) backRightLeg.transform.localEulerAngles = swingForward; 

        if (frontRightLeg) frontRightLeg.transform.localEulerAngles = swingBack;
        if (backLeftLeg) backLeftLeg.transform.localEulerAngles = swingBack;
    }

    #region UI
    public void ShowFloatingText(int amount)
    {
        if (floatingTextPrefab == null) return;

        GameObject ft = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
        ft.transform.LookAt(Camera.main.transform); // face camera
        ft.transform.Rotate(0, 180f, 0); // fix text facing

        if(ft.TryGetComponent<FloatingText>(out FloatingText floatingText)) floatingText.SetText("+" + amount);
    }

    #endregion

    #region SAVEDATA
    public PigSaveData GetSaveData()
    {
        return new PigSaveData
        {
            posX = transform.position.x, posY = transform.position.y, posZ = transform.position.z,

            rotX = transform.rotation.x, rotY = transform.rotation.y, rotZ = transform.rotation.z, rotW = transform.rotation.w,

            hp = hp, mhp = mhp,
            pigType = (int)pigType,
            beenKicked = beenKicked
        };
    }
    public void LoadFromSaveData(PigSaveData data)
    {
        transform.SetPositionAndRotation(
            new Vector3(data.posX, data.posY, data.posZ),
            new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW)
            );

        hp = data.hp;
        mhp = data.mhp;

        pigType = (PigType)data.pigType;
        beenKicked = data.beenKicked;


    }
    #endregion

    void OnDrawGizmosSelected()
    {
        // only draw in editor when pig is selected
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // semi-transparent green
        Gizmos.DrawSphere(transform.position, wanderRadius);

        // optional: draw a line to current target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawSphere(wanderTarget, 0.1f);
        }
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        if(combat)
            Gizmos.DrawSphere(transform.position, combat.cannibalRadius);
    }
}