using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Pig : MonoBehaviour
{
    public GameObject baconPrefab;
    public Transform spawnPoint;
    
    public float kickForce = 5f; // how strongly the pig flies back
    public float uprightDelay = 1f; // seconds to stand back up
    
    public float wanderRadius = 2f; // max random offset for wandering
    public float wanderChangeInterval = 2f; // seconds between direction changes
    public GameObject pigdust;

    public AudioClip oinkSound; // assign a pig oink sound in inspector
    public AudioClip eatSound;
    private AudioSource audioSource;

    private Rigidbody rb;
    private bool isUpright = true;
    private Vector3 wanderTarget;
    private Vector3 wanderCenter = new (2.02f, 0.43f, 1.54f);

    [Header("Stats")]
    public int hp = 30;
    public int mhp = 30;
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
    public bool isDevil = false;
    public bool isBoar = false;
    public float cannibalRadius = 1f;
    private int currentPhase = 1;
    private int fastSpecial = 4;
    private int slowSpecial = 8;

    void Awake()
    {
        if (isDevil)
        {
            if (BossUI == null)
                BossUI = GameObject.Find("BossUI");

            if (HPBar == null && BossUI != null)
                HPBar = BossUI.GetComponentInChildren<Slider>();
        }
    }

    void Start()
    {
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
        if(isDevil)
        {
            DayCycle.instance.bossFight = true;
            BossUI.SetActive(true);
            HPBar.maxValue = mhp;
            StartCoroutine(BossAttackRoutine());
        }
        if (isBoar)
        {
            StartCoroutine(BoarRoutine());
        }
    }

    void Update()
    {
        if (isDevil)
        {
            if ((hp <= (mhp / 2)) && currentPhase == 1)
            {
                currentPhase = 2;
                fastSpecial = 3;
                slowSpecial = 6;
            }
            else if ((hp <= (mhp / 10)) && currentPhase == 2)
            {
                currentPhase = 3;
                fastSpecial = 3;
                slowSpecial = 4;
            }
            UpdateHPBar();
        }

        if (isUpright)
        {

            //bool isMoving = rb.velocity.magnitude > 0.05f;
            if (!isDevil)
            {
                AnimateLegs(true);
            }
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

        // spawn bacon
        for (int i = 0; i < baconPerKick; i++)
        {
            GameObject newBacon = Instantiate(baconPrefab, spawnPoint.position, Quaternion.identity);
            Bacon bacon = newBacon.GetComponentInChildren<Bacon>();
            bacon.baconValue = damage;
        }

        // apply knockback force
        if (rb != null && kicker != null)
        {
            Vector3 forceDir = (transform.position - kicker.position).normalized + Vector3.up * 0.3f;
            rb.AddForce(forceDir * kickForce, ForceMode.Impulse);
        }

        hp -= damage;
        if (!beenKicked)
        {
            beenKicked = true;
            GameManager.instance.UpdateKickedPigs();
        }

        
        // start upright coroutine
        StartCoroutine(StandUpright());
        
    }
    private void Die()
    {
        if (pigdust != null)
        {
            GameObject particles = Instantiate(pigdust, transform.position, Quaternion.identity);
            Destroy(particles, 1f); // clean up after 1 second
        }
        GameManager.instance.pigsCount--;
        GameManager.instance.UpdatePigCount();
        if (isDevil)
        { 
            DayCycle.instance.bossFight = false;
            BossUI.SetActive(false);
            
        }
        if(pigType == PigType.Angel)
        {
            GameManager.instance.angelKills++;
            if(GameManager.instance.angelKills >= 3)
            {
                ShopManager.instance.DevilButton.GetComponent<Button>().interactable = true;
                ShopManager.instance.DevilCostText.text = "Devil Pig - 3000 bacon";
            }
            else
            {
                ShopManager.instance.DevilButton.SetActive(true);
                ShopManager.instance.DevilButton.GetComponent<Button>().interactable = false;
                ShopManager.instance.DevilCostText.text = "Devil Pig - (" + GameManager.instance.angelKills + "/3 angels killed.";
            }
        }

        Destroy(gameObject);
    }
    private IEnumerator StandUpright()
    {
        yield return new WaitForSeconds(uprightDelay);
        if (hp <= 0) 
        { 
            Die();  
        }
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            Quaternion uprightRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            float timer = 0f;
            float duration = 0.2f;

            while (timer < duration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, uprightRotation, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.rotation = uprightRotation;
        }

        isUpright = true; // pig can now wander
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
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (isUpright)
            {
                // pick a new random target near current position
                wanderTarget = wanderCenter + new Vector3(
                    Random.Range(-wanderRadius, wanderRadius),
                    0f,
                    Random.Range(-wanderRadius, wanderRadius)
                );
            }

            yield return new WaitForSeconds(wanderChangeInterval);
        }
    }


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
        Gizmos.DrawSphere(transform.position, cannibalRadius);
    }
    void HandleMovement()
    {
        if (carrotTarget != null)
        {
            // Move toward carrot
            Vector3 direction = (carrotTarget.position - transform.position);
            direction.y = 0f;

            if (direction.magnitude > eatDistance)
            {
                rb.MovePosition(transform.position + direction.normalized * wanderSpeed * Time.deltaTime);
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
            }
            else
            {
                // Eat carrot
                if (CarrotPatchGold.instance != null && CarrotPatchGold.instance.currentCarrot == carrotTarget.gameObject)
                {
                    hp += 10;
                    ShowFloatingText(10);
                    Debug.Log("Gold Carrot eaten");
                }
                else { hp += 2; Debug.Log("Carrot eaten"); ShowFloatingText(2); }
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
                rb.MovePosition(transform.position + direction.normalized * wanderSpeed * Time.deltaTime);
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
    private void ShowFloatingText(int amount)
    {
        if (floatingTextPrefab == null) return;

        GameObject ft = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
        ft.transform.LookAt(Camera.main.transform); // face camera
        ft.transform.Rotate(0, 180f, 0); // fix text facing
        FloatingText floatingText = ft.GetComponent<FloatingText>();
        if (floatingText != null)
            floatingText.SetText("+" + amount);
    }

    #region Boss and Specials
    private IEnumerator BossAttackRoutine()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(fastSpecial, slowSpecial));
            int attack = 1;//Random.Range(1, 3); //0-4
            switch (attack)
            {
                case 0:
                    Debug.Log("Carrot Rain");
                    yield return StartCoroutine(CarrotRain());
                    break;
                case 1:
                    Debug.Log("Lightning Storm");
                    yield return StartCoroutine(LightningStorm());
                    break;
                case 2:
                    Debug.Log("Cannibal Hunger");
                    yield return StartCoroutine(CannibalHunger());
                    break;
                case 3:
                    Debug.Log("Charge");
                    yield return StartCoroutine(ChargeAttack());
                    break;
            }
        }
    }
    IEnumerator CarrotRain()
    {
        DayCycle.instance.Flash();
        
        Vector3 center = Center.instance.transform.position;
        float radius = Center.instance.radius;

        int total = Random.Range(10, 16);
        int goldCount = Random.Range(2, 5);

        for (int i = 0; i < total; i++)
        {
            Vector2 offset2D = Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(
                offset2D.x,
                9f,
                offset2D.y
            );

            GameObject prefabToUse = (goldCount > 0 && Random.value < 0.25f)
                    ? CarrotPatchGold.instance.carrotPrefab
                    : CarrotPatch.instance.carrotPrefab;

                Instantiate(prefabToUse, pos, Quaternion.identity);

                if (prefabToUse == CarrotPatchGold.instance.carrotPrefab)
                    goldCount--;

                yield return new WaitForSeconds(0.1f);
        }
    }
    public GameObject lightning_warning;
    public GameObject lightningPrefab;
    IEnumerator LightningStorm()
    {
        DayCycle.instance.Darken();

        Vector3 center = Center.instance.transform.position;
        float radius = Center.instance.radius + 0.5f; // +1 for lightning

        int bodyLayer = LayerMask.NameToLayer("BODY");
        int strikes = Random.Range(4, 7);

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        for (int i = 0; i < strikes; i++)
        {
            Vector3 strikePos;
            // 65% chance to bias toward player
            if (player != null && Random.value < 0.65f)
            {
                Vector3 offset = new 
                (
                    Random.Range(-2.5f, 2.5f),
                    0f,
                    Random.Range(-2.5f, 2.5f)
                );

                strikePos = player.position + offset;
                strikePos.y = 0.1f;

                // clamp back into arena radius if needed
                Vector3 fromCenter = strikePos - center;
                fromCenter.y = 0f;

                if (fromCenter.magnitude > radius)
                {
                    fromCenter = fromCenter.normalized * radius;
                    strikePos = center + fromCenter;
                    strikePos.y = 0.1f;
                }
            }
            else
            {
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                strikePos = center + new Vector3(randomCircle.x, 0.1f, randomCircle.y);
            }

            GameObject warning = null;

            if(lightning_warning != null)
            {
                warning = Instantiate(lightning_warning, strikePos, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.4f);
            if (warning != null)
            {
                Destroy(warning);
            }
            DayCycle.instance.Flash();

            if (lightningPrefab != null)
            {
                Instantiate(lightningPrefab, strikePos, Quaternion.identity);
            }

            Collider[] hits = Physics.OverlapSphere(strikePos, 1.5f);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.layer == bodyLayer)
                {
                    int currentBacon = GameManager.instance.baconCount;

                    int loss = Mathf.Max(200, currentBacon / 10);
                    if (currentBacon < loss) loss = currentBacon;
                    if (loss == 0)
                    {
                        
                        break;
                    }
                    
                    int spill = loss / 2;

                    // remove bacon
                    GameManager.instance.SpendBacon(loss);

                    // spill bacon physically
                    for (int b = 0; b < spill; b++)
                    {
                        GameObject newBacon = Instantiate(baconPrefab, strikePos, Quaternion.identity);
                        Rigidbody brb = newBacon.GetComponent<Rigidbody>();

                        if (brb != null)
                        {
                            Vector3 dir = new Vector3(
                                Random.Range(-1f, 1f),
                                1f,
                                Random.Range(-1f, 1f)
                            ).normalized;

                            brb.AddForce(dir * Random.Range(2f, 5f), ForceMode.Impulse);
                        }
                    }

                    break; // only apply once
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
    }
    IEnumerator CannibalHunger()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, cannibalRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Bacon"))
            {
                StartCoroutine(SuckBacon(hit.gameObject));
                //Destroy(hit.gameObject);
                //hp += 1;
                //ShowFloatingText(1);
            }
        }

        // Chance to eat another pig
        Pig[] pigs = FindObjectsByType<Pig>(FindObjectsSortMode.None);

        foreach (Pig p in pigs)
        {
            if (p != this && Random.value < 0.3f)
            {
                carrotTarget = p.transform; // reuse targeting system
                break;
            }
        }

        yield return null;
    }
    IEnumerator SuckBacon(GameObject bacon)
    {
        if (bacon == null) yield break;

        float duration = 0.35f;
        float timer = 0f;

        Vector3 startPos = bacon.transform.position;

        // Optional: disable physics so it doesn't fight you
        if (bacon.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        while (timer < duration)
        {
            if (bacon == null) yield break;

            timer += Time.deltaTime;
            float t = timer / duration;

            // Smooth curve (feels more "sucky")
            t = Mathf.SmoothStep(0f, 1f, t);

            bacon.transform.position = Vector3.Lerp(startPos, transform.position, t);

            // shrink while flying in
            bacon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            yield return null;
        }

        // Finalize
        if (bacon != null)
        {
            Destroy(bacon);
            hp += 1;
            ShowFloatingText(1);
        }
    }
    IEnumerator ChargeAttack()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) yield break;

        int bodyLayer = LayerMask.NameToLayer("BODY");

        Vector3 dir = (player.position - transform.position).normalized;

        float chargeTime = 1.5f;
        float speed = 8f;

        float timer = 0f;

        while (timer < chargeTime)
        {
            rb.MovePosition(transform.position + dir * speed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Check hit at end of charge
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject.layer == bodyLayer)
            {
                int currentBacon = GameManager.instance.baconCount;

                int loss = isDevil ? Mathf.Max(200, currentBacon / 10) : Mathf.Max(100, currentBacon / 20);
                int spill = loss / 2;

                GameManager.instance.SpendBacon(loss);

                // spill bacon
                for (int b = 0; b < spill; b++)
                {
                    GameObject newBacon = Instantiate(baconPrefab, transform.position, Quaternion.identity);
                    Rigidbody brb = newBacon.GetComponent<Rigidbody>();

                    if (brb != null)
                    {
                        Vector3 randDir = new Vector3(
                            Random.Range(-1f, 1f),
                            1f,
                            Random.Range(-1f, 1f)
                        ).normalized;

                        brb.AddForce(randDir * Random.Range(3f, 6f), ForceMode.Impulse);
                    }
                }

                break;
            }
        }
    }
    #endregion
    IEnumerator BoarRoutine()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            yield return StartCoroutine(ChargeAttack());
        }
    }
    public GameObject BossUI;
    public Slider HPBar;
    public void UpdateHPBar()
    {
        float current = Mathf.Clamp(hp, 0, mhp);
        HPBar.value = current;
    }

    public PigSaveData GetSaveData()
    {
        return new PigSaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,

            rotX = transform.rotation.x,
            rotY = transform.rotation.y,
            rotZ = transform.rotation.z,
            rotW = transform.rotation.w,

            hp = hp,
            mhp = mhp,
            baconPerKick = baconPerKick,
            pigType = (int)pigType,
            beenKicked = beenKicked
        };
    }

    public void LoadFromSaveData(PigSaveData data)
    {
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

        hp = data.hp;
        mhp = data.mhp;
        baconPerKick = data.baconPerKick;
        pigType = (PigType)data.pigType;
        beenKicked = data.beenKicked;

        isBoar = pigType == PigType.Boar;
        isDevil = pigType == PigType.Devil;
    }
}

