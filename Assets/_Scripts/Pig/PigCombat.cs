using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PigCombat : MonoBehaviour
{
    public Pig pig;

    private AudioSource audioSource;

    private Rigidbody rb;

    [Header("Lightning")]
    public GameObject lightning_warning;
    public GameObject warningParticles;
    public GameObject lightningPrefab;
    
    [Header("Charge")]
    public bool charging = false;
    public ParticleSystem chargeDust;

    [Header("Devil")]
    private float fastSpecial = 4f;
    private float slowSpecial = 8f;
    public float cannibalRadius = 1f;
    private int currentPhase = 1;
    public GameObject carrot;
    public GameObject goldCarrot;
    public GameObject BossUI;
    public Slider HPBar;

    [Header("Player Target")]
    Transform player;

    void Awake()
    {
        if (pig.pigType == PigType.Devil)
        {
            if (BossUI == null)
                BossUI = GameObject.Find("BossUI");

            if (HPBar == null && BossUI != null)
                HPBar = BossUI.GetComponentInChildren<Slider>();
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (pig.pigType == PigType.Devil)
        {
            DayCycle.instance.bossFight = true;
            BossUI.SetActive(true);
            HPBar.maxValue = pig.mhp;
            StartCoroutine(BossAttackRoutine());
        }
        if (pig.pigType == PigType.Cyboarg)
        {
            StartCoroutine(CyboargRoutine());
            return;
        }
        if (pig.pigType == PigType.Boar)
        {
            StartCoroutine(BoarRoutine());
        }
        if (pig.pigType == PigType.Cyborg)
        {
            StartCoroutine(CyberRoutine());
        }
    }
    private void Update()
    {
        if (pig.pigType == PigType.Devil)
        {
            if ((pig.hp <= (pig.mhp / 2)) && currentPhase == 1)
            {
                currentPhase = 2;
                fastSpecial = 3f; slowSpecial = 6f;
            }
            else if ((pig.hp <= (pig.mhp / 10)) && currentPhase == 2)
            {
                currentPhase = 3;
                fastSpecial = 3f; slowSpecial = 4f;
            }
            UpdateHPBar();
        }
    }
    public Coroutine StartCharge()
    {
        if (chargeRoutine != null)
            StopCoroutine(chargeRoutine);

        chargeRoutine = StartCoroutine(ChargeAttack());
        return chargeRoutine;
    }
    public void StopCharge()
    {
        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
            chargeRoutine = null;
        }
        chargeHitbox.Deactivate();
        Destroy(currentDash);
        currentDash = null;
        charging = false;
    }
    IEnumerator ChargeAttack()
    {
        if (player == null) 
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            yield break; 
        }

        charging = true;
        chargeDust.Play();
        yield return new WaitForSeconds(0.7f);

        if (audioSource != null && chargeSqueal != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.15f);
            audioSource.PlayOneShot(chargeSqueal);
        }

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        dir = dir.normalized;

        float chargeTime = pig.pigType == PigType.Devil ? 2.1f : 1.8f;
        float speed = pig.pigType == PigType.Devil ? 25f : 18f;
        float timer = 0f;

        chargeHitbox.Activate();

        Debug.Log("Instantiating Dash prefab");
        Vector3 offset = transform.up * 0.35f + transform.forward * 1.08f;
        Vector3 instantiatePos = (pig.pigType == PigType.Devil ? transform.position + offset : transform.position);
        currentDash = Instantiate(
            dashPrefab,
            instantiatePos,
            transform.rotation * Quaternion.Euler(0, 180f, 0)
        );

        currentDash.transform.SetParent(transform, true);
        currentDash.transform.position += transform.forward * 1f;
        //Debug.Log("Dash position: " + currentDash.transform.position);

        Quaternion targetRot = Quaternion.LookRotation(dir);
        int fenceMask = 1 << 8;
        while (timer < chargeTime)
        {
            float step = speed * Time.fixedDeltaTime;
            Vector3 origin = rb.position + Vector3.up * 0.5f;
            float radius = 0.75f;
            float distance = step + radius + 0.25f;

            bool hitFence = Physics.SphereCast(
                origin,
                radius,
                dir,
                out RaycastHit hit,
                distance,
                fenceMask,
                QueryTriggerInteraction.Ignore
            );

            if (hitFence)
            {
                rb.MovePosition(rb.position - dir * 0.3f);
                break;
            }

            rb.MovePosition(rb.position + dir * step);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        chargeHitbox.Deactivate();

        if (currentDash != null)
        {
            Destroy(currentDash);
            currentDash = null;
        }

        charging = false;
    }
    IEnumerator LightningStorm(bool useCenter=false)
    {
        DayCycle.instance.Darken();

        Vector3 center = Center.instance.transform.position;
        float radius = Center.instance.radius + 0.5f; // +1 for lightning

        int bodyLayer = LayerMask.NameToLayer("BODY");

        int minStrikes = pig.pigType == PigType.Devil ? 4 : 1;
        int maxStrikes =
            pig.pigType == PigType.Devil
                ? 7
                : (pig.pigType == PigType.Cyboarg)
                    ? 4
                    : 3;
        

        int strikes = Random.Range(minStrikes, maxStrikes);
        if (useCenter) { strikes = 1; }

        Transform player = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < strikes; i++)
        {
            Vector3 strikePos;
            // 45% chance to bias toward player
            if (player != null && Random.value < 0.45f && !useCenter)
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
            else if(!useCenter)
            {
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                strikePos = center + new Vector3(randomCircle.x, 0.1f, randomCircle.y);
            }
            else
            {
                strikePos = center;
            }
            GameObject warning = null;

            if (lightning_warning != null)
            {
                warning = Instantiate(lightning_warning, strikePos, Quaternion.identity);
                Instantiate(warningParticles, strikePos, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.5f);
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
                    int loss = pig.pigType == PigType.Devil
                        ? Mathf.Max(200, GameManager.instance.baconCount / 10)
                        : Mathf.Max(50, GameManager.instance.baconCount / 20);
                    DamagePlayer(strikePos, loss);
                    PlayerController playerBody = hit.GetComponentInParent<PlayerController>();
                    if (playerBody != null)
                    {
                        Vector3 knockDir = (hit.transform.position - strikePos);
                        playerBody.ApplyKnockback(knockDir, 5f, 20f);
                    }
                    else
                    {
                        Debug.LogWarning("Lightning hit BODY collider but no PlayerController found in parent.");
                    }
                    break; // only apply once
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
    }
    public GameObject dashPrefab;
    public ChargeHitbox chargeHitbox;
    public AudioClip chargeSqueal;
    private Coroutine chargeRoutine;
    private GameObject currentDash;

    public void DamagePlayer(Vector3 hitPosition, int loss)
    {
        Damage.instance.BaconDamage(loss, hitPosition);
        GameManager.instance.UpdateScore(-20);
    }

    #region BOSS SPECIALS
    IEnumerator CannibalHunger()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, cannibalRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Bacon"))
            {
                StartCoroutine(SuckBacon(hit.gameObject));
            }
        }

        // Chance to eat another pig
        /*Pig[] pigs = FindObjectsByType<Pig>(FindObjectsSortMode.None);

        foreach (Pig p in pigs)
        {
            if (p != this && Random.value < 0.3f)
            {
                //rewrite this
                //pig.carrotTarget = p.transform; // reuse targeting system
                break;
            }
        }*/

        yield return null;
    }
    IEnumerator SuckBacon(GameObject bacon)
    {
        if (bacon == null) yield break;

        float duration = 0.9f;
        float timer = 0f;

        Vector3 startPos = bacon.transform.position;

        // Optional: disable physics so it doesn't fight you
        if (bacon.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

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

            int healing = bacon.GetComponentInChildren<Bacon>().baconValue;

            pig.hp += healing;
            Destroy(bacon);
            pig.ShowFloatingText(healing);
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
                    ? goldCarrot
                    : carrot;

            Instantiate(prefabToUse, pos, Quaternion.identity);

            if (prefabToUse == goldCarrot)
                goldCount--;

            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region BEHAVIOUR ROUTINES
    public IEnumerator BoarRoutine()
    {
        while (pig.hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if (pig.isUpright)
            {
                yield return StartCharge();
            }
        }
    }
    public IEnumerator CyberRoutine()
    {
        while (pig.hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(5f, 12f));
            yield return StartCoroutine(LightningStorm());
        }
    }
    public IEnumerator CyboargRoutine()
    {
        Debug.Log("CyBOARG");
        while (pig.hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(4f, 9f));
            int attack = Random.Range(0, 2); //0-1
            switch (attack)
            {
                case 0:
                    Debug.Log("Lightning Storm");
                    yield return StartCoroutine(LightningStorm());
                    break;

                case 1:
                    Debug.Log("Charge");
                    if (pig.isUpright)
                    {
                        yield return StartCharge();
                    }
                    break;
            }
        }
        if (pig.hp > 0) Debug.LogWarning("CyBOARg improper exit");
    }
    public IEnumerator BossAttackRoutine()
    {
        Debug.Log("BossAttackRoutine started");
        //fastSpecial = 1f;
        //slowSpecial = 1.5f;
        while (pig.hp > 0)
        {
            yield return new WaitForSeconds(Random.Range(fastSpecial, slowSpecial));
            int attack;
            if (GameManager.instance.carrotCount <= 30)
            {
                attack = Random.Range(0, 4);
            }
            else attack = Random.Range(1, 4);
            yield return StartCoroutine(RunAttackSafely(attack)); 
        }

        Debug.Log("BossAttackRoutine ended because hp <= 0");
    }
    private IEnumerator RunAttackSafely(int attack)
    {
        IEnumerator routine = null;

        switch (attack)
        {
            case 0:
                Debug.Log("Boss attack: Carrot Rain");
                routine = CarrotRain();
                break;

            case 1:
                Debug.Log("Boss attack: Lightning Storm");
                routine = LightningStorm();
                break;

            case 2:
                Debug.Log("Boss attack: Cannibal Hunger");
                routine = CannibalHunger();
                break;

            case 3:
                Debug.Log("Boss attack: Charge");
                routine = ChargeAttack();
                break;
        }

        if (routine == null)
            yield break;

        bool finished = false;

        yield return StartCoroutine(RunWithCatch(routine, () => finished = true));

        if (!finished)
        {
            Debug.LogError("Boss attack FAILED: " + attack);
        }
    }

    private IEnumerator RunWithCatch(IEnumerator routine, System.Action onSuccess)
    {
        while (true)
        {
            object current;

            try
            {
                if (!routine.MoveNext())
                {
                    onSuccess?.Invoke();
                    yield break;
                }

                current = routine.Current;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Coroutine crashed: " + e);
                yield break;
            }

            yield return current;
        }
    }
    #endregion

    public void UpdateHPBar()
    {
        float current = Mathf.Clamp(pig.hp, 0, pig.mhp);
        HPBar.value = current;
    }
}