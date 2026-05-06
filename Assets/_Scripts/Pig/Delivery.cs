using System.Collections;
using UnityEngine;

public class Delivery : MonoBehaviour
{
    public GameObject UFO;
    public GameObject cratePrefab;

    public Transform spawnpos;
    public Transform endpos;

    [Header("Timing")]
    public float time = 0f;
    public float spawnThreshold = 297f;
    public float flyDuration = 6f;

    [Header("Drop")]
    public Transform dropPoint; // where the crate should appear

    private bool isFlying = false;
    private bool hasDropped = false;

    private void Start()
    {
        UFO.SetActive(false);
    }

    private void Update()
    {
        if (!ShopManager.instance.deliver) return;
        if (isFlying)
            return;

        time += Time.deltaTime;

        if (time >= spawnThreshold)
        {
            time = 0f; // reset timer for next delivery
            
            StartCoroutine(FlyAndDrop());
        }
    }

    IEnumerator FlyAndDrop()
    {
        isFlying = true;
        hasDropped = false;

        UFO.SetActive(true);
        UFO.transform.position = spawnpos.position;

        float halfDuration = flyDuration * 0.5f;

        // --- PHASE 1: spawn → drop point ---
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            UFO.transform.position = Vector3.Lerp(
                spawnpos.position,
                dropPoint.position,
                t
            );

            yield return null;
        }

        UFO.transform.position = dropPoint.position;
        if (GameManager.instance.crateCount < 5)
            {// --- DROP CRATE ---
            if (!hasDropped)
            {
                hasDropped = true;
                Instantiate(cratePrefab, dropPoint.position, Quaternion.identity); 
            
            }

            // small pause for effect
            yield return new WaitForSeconds(0.3f);
        }

        // --- PHASE 2: drop point → end ---
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            UFO.transform.position = Vector3.Lerp(
                dropPoint.position,
                endpos.position,
                t
            );

            yield return null;
        }

        UFO.transform.position = endpos.position;

        // --- CLEANUP ---
        UFO.SetActive(false);
        isFlying = false;
    }
}