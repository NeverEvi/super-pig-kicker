using UnityEngine;

public class CarrotPatches : MonoBehaviour
{
    public static CarrotPatches instance;

    [System.Serializable]
    public class PatchData
    {
        public Transform spawnPoint;
        public GameObject carrotPrefab;
        public float spawnInterval = 60f;

        [HideInInspector] public GameObject currentCarrot;
        [HideInInspector] public float timer;
    }

    [Header("Patches")]
    public PatchData normalPatch;
    public PatchData goldPatch;

    private void Awake() => instance = this;

    private void Update()
    {
        UpdatePatch(normalPatch);
        if (ShopManager.instance.cyborgBought)
        {
            UpdatePatch(goldPatch);
        }
    }

    private void UpdatePatch(PatchData patch)
    {
        if (patch.spawnPoint == null || patch.carrotPrefab == null) return;

        if (patch.currentCarrot == null)
        {
            patch.timer += Time.deltaTime;

            if (patch.timer >= patch.spawnInterval)
            {
                SpawnCarrot(patch);
                patch.timer = 0f;
            }
        }
    }

    private void SpawnCarrot(PatchData patch)
    {
        Vector3 spawnPos = patch.spawnPoint.position;
        spawnPos.y += 0.1f;

        patch.currentCarrot = Instantiate(
            patch.carrotPrefab,
            spawnPos,
            Quaternion.Euler(90f, 0f, 0f)
        );

        if (patch.currentCarrot.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void ClearCarrot(GameObject carrot)
    {
        if (normalPatch.currentCarrot == carrot)
        {
            normalPatch.currentCarrot = null;
        }
        else if (goldPatch.currentCarrot == carrot)
        {
            goldPatch.currentCarrot = null;
        }
    }
}