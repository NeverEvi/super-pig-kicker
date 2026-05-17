using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86;

public class Pig : MonoBehaviour
{
    public Transform spawnPoint;

    public PigCombat combat;

    public GameObject pigdust;

    [Header("Wander")]

    public float wanderRadius = 2f; // max random offset for wandering
    public float minWanderChangeInterval = 2.5f; // seconds between direction changes
    public float maxWanderChangeInterval = 5f; // seconds between direction changes

    private Vector3 wanderTarget;
    private Vector3 wanderCenter = new (2.02f, 0.43f, 1.54f);

    [Header("Kicking")]
    private Rigidbody rb;
    public float kickForce = 5f; // how strongly the pig flies back
    
    public bool isUpright = true;
    private readonly float uprightDelay = 0.6f; // seconds to stand back up
    private Coroutine standUprightRoutine;
    public GameObject SSK;
    public GameObject SPK;

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

    public float legSwingAmplitude = 22f; // max rotation angle
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
    public AudioClip Crit1;
    public AudioClip Crit2;

    private Transform player;

    void Start()
    {
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "SPINKICK")
            {
                SSK = go;
                break;
            }
        }
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "SUPERKICK")
            {
                SPK = go;
                break;
            }
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

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
        if (!isUpright) return;

        balanceCheckTime += Time.deltaTime;


        if (pigType != PigType.Devil) AnimateLegs(true);

        
        if (balanceCheckTime > 10f)
        {
            balanceCheckTime = 0f;

            float uprightDot = Vector3.Dot(transform.up, Vector3.up);

            if (uprightDot < 0.15f) StartStandUpright();
        }

        HandleMovement();
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
            Vector3 awayDir = (transform.position - kicker.position).normalized;

            float spinStrength = 1f;
            float totalKickForce = kickForce;
            float upwardForce = 4f;

            //Vector3 forceDir = ((transform.position - kicker.position) + Vector3.up * 2f).normalized;
            float SSK_Chance = Random.value;
            float SPK_Chance = Random.value;
            int critSound = Random.Range(1, 3);
            if (SPK_Chance < GameManager.instance.SPK)
            {
                

                AudioHelper.PlayClipAtPosition((critSound == 1) ? Crit1 : Crit2, transform.position, 1f, Random.Range(0.9f, 1.1f));
                SPK.SetActive(true);

                spinStrength = 2.2f; totalKickForce += 2f; upwardForce = 5f;
                
                int top = 1;
                rb.maxAngularVelocity = 100f;
                rb.angularVelocity = Random.onUnitSphere * 20f;
                switch (pigType)
                {
                    case (PigType.Pig):
                        top = 2;
                        break;
                    case (PigType.Boar):
                        top = 3;
                        break;
                    case (PigType.Golden):
                        top = 4;
                        break;
                    case (PigType.Alien):
                        top = 5;
                        break;
                    case (PigType.Cyborg):
                        top = 6;
                        break;
                    case (PigType.Cyboarg):
                        top = 7;
                        break;
                    case (PigType.Angel):
                        top = 8;
                        break;
                    case (PigType.Devil):
                        top = 10;
                        break;
                }
                top += GameManager.instance.newGamePlus;

                Damage.instance.BaconDamage(Random.Range(1,top+1), Vector3.zero, this);
            }
            else if (SSK_Chance < GameManager.instance.SSK)
            {
                AudioHelper.PlayClipAtPosition((critSound == 1) ? Crit1 : Crit2, transform.position, 1f, Random.Range(0.9f, 1.1f));
                SSK.SetActive(true);

                spinStrength = 90f;
                totalKickForce += 0.2f;
                upwardForce = 4.2f;
                rb.maxAngularVelocity = 100f;
                rb.angularVelocity = Random.onUnitSphere * 50f;

                Damage.instance.BaconDamage(2, Vector3.zero, this);
            }

            rb.AddForce(awayDir * totalKickForce, ForceMode.Impulse);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * spinStrength, ForceMode.Impulse);
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
    private bool IsGrounded()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        Vector3 localCenter = box.center + Vector3.forward * 0.05f;
        Vector3 worldCenter = transform.TransformPoint(localCenter);

        Vector3 halfExtents = (box.size * 0.5f) + new Vector3(0.05f, 0.05f, 0.15f);

        Collider[] hits = Physics.OverlapBox(
            worldCenter,
            halfExtents,
            transform.rotation,
            LayerMask.GetMask("Ground")
        );

        return hits.Length > 0;
    }
    public IEnumerator StandUpright()
    {
        yield return new WaitForSeconds(uprightDelay/2);

        while (!IsGrounded())
        {
            yield return null;
        }
        yield return new WaitForSeconds(uprightDelay);

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            Quaternion uprightRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            float timer = 0f;
            float duration = 0.8f;
            rb.AddForce(Vector3.up * 1.2f, ForceMode.Impulse);
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
    private Vector3 rawWanderTarget;
    private float balanceCheckTime = 0f;
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            float nextWanderWait = Random.Range(minWanderChangeInterval, maxWanderChangeInterval);
            if (isUpright)
            {
                Vector3 randomOffset = new(
                    Random.Range(-wanderRadius, wanderRadius),
                    0f,
                    Random.Range(-wanderRadius, wanderRadius)
                );

                rawWanderTarget = wanderCenter + randomOffset;
                if (beenKicked 
                    && pigType!=PigType.Boar 
                    && pigType != PigType.Cyboarg 
                    && pigType != PigType.Devil) 
                {
                    Vector3 awayFromPlayer = (transform.position - player.transform.position).normalized;

                    // VERY slight influence (tweak this)
                    float avoidStrength = 0.35f;

                    randomOffset += awayFromPlayer * wanderRadius * avoidStrength;
                }
                wanderTarget = wanderCenter + randomOffset;

                
            }

            yield return new WaitForSeconds(nextWanderWait);
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
        Gizmos.DrawSphere(wanderCenter, wanderRadius);

        // optional: draw a line to current target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawSphere(wanderTarget, 0.1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, rawWanderTarget);
            Gizmos.DrawWireSphere(rawWanderTarget, 0.1f);
        }
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        if(combat)
            Gizmos.DrawSphere(transform.position, combat.cannibalRadius);

        if (!TryGetComponent<BoxCollider>(out BoxCollider box)) return;

        Vector3 localCenter = box.center + Vector3.forward * 0.05f;
        Vector3 worldCenter = transform.TransformPoint(localCenter);

        Vector3 size = box.size + new Vector3(0.1f, 0.1f, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, size);

    }
}