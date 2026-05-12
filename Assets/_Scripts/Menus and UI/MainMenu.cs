using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    public GameObject player;
    public GameObject BaconCounter;
    public GameObject kickStrength;
    public GameObject fps;
    public GameObject mainmenu;
    public GameObject spawnPoint;
    public GameObject devilStartPoint;
    public GameObject UI_PANEL;
    public GameObject title;
    public GameObject kickTutorial;
    public Image blackout;
    private GameObject crateInstance;

    [Header("Fade")]
    public float fadeDuration = 0.75f;

    [Header("Prefabs")]
    public GameObject cratePrefab;
    public GameObject pigPrefab;
    public GameObject boarPrefab;
    public GameObject goldenPigPrefab;
    public GameObject alienPigPrefab;
    public GameObject cyborgPigPrefab;
    public GameObject angelPigPrefab;
    public GameObject devilPrefab;
    public GameObject potatoPrefab;

    public Vector3 spawnpos = new (1.05110359f, 10.8000002f, 2.64636302f);
    private Vector3 potatopos = new Vector3(-3.89945507f, 12.56200004f, 10.1427422f);
    PlayerController pc;
    private bool isStarting = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetBlackoutAlpha(0);
        pc = player.GetComponent<PlayerController>();
        crateInstance = Instantiate(cratePrefab, spawnpos, Quaternion.identity);
        crateInstance.SetActive(false);
    }

    public void NewGame()
    {
        if (isStarting) return;
        StartCoroutine(NewGameRoutine());
    }

    public void Continue()
    {
        if (isStarting) return;
        StartCoroutine(ContinueRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        isStarting = true;

        yield return StartCoroutine(FadeBlackout(0f, 1f));

        SaveSystem.DeleteSave();

        StartGameplay();

        pc.enabled = false;
        pc.canMove = false;
        

        BaconCounter.SetActive(true);
        kickStrength.SetActive(true);
        fps.SetActive(true);

        
        player.transform.SetPositionAndRotation(
            spawnPoint.transform.position,
            new Quaternion(0f, -0.449043423f, 0f, 0.893509984f)
        );
        StartCoroutine(SpawnNGPlusPotatoes(GameManager.instance.newGamePlus));
        mainmenu.SetActive(false);
        title.SetActive(true);
        
        ShopManager.instance.playing = true;
        Rigidbody rb = crateInstance.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        crateInstance.SetActive(true);
        yield return null;
        rb.isKinematic = false;

        yield return StartCoroutine(FadeBlackout(1f, 0f));
        
        kickTutorial.SetActive(true);
        yield return StartCoroutine(EnableControls());
        isStarting = false;
        
    }

    private IEnumerator ContinueRoutine()
    {
        Debug.Log("Continue routine start");
        isStarting = true;

        yield return StartCoroutine(FadeBlackout(0f, 1f));

        SaveData data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.Log("No save data");
            yield return StartCoroutine(NewGameRoutine());
            yield break;
        }

        StartGameplay();
        ShopManager.instance.playing = true;
        ClearRuntimeObjects();

        LoadPlayer(data.player);
        LoadGameManager(data.gameManager);

        ShopManager.instance.LoadFromSaveData(data.shop);

        if (CarrotPatches.instance != null)
        {
            CarrotPatches.instance.normalPatch.spawnInterval = data.carrotPatchInterval;
            CarrotPatches.instance.goldPatch.spawnInterval = data.goldCarrotPatchInterval;
        }


        if (PassiveBacon.instance != null)
            PassiveBacon.instance.spawnInterval = data.passiveBaconInterval;

        foreach (CrateSaveData crateData in data.crates)
        {
            GameObject crateObj = Instantiate(
                cratePrefab,
                new Vector3(crateData.posX, crateData.posY, crateData.posZ),
                new Quaternion(crateData.rotX, crateData.rotY, crateData.rotZ, crateData.rotW)
            );

            if(crateObj.TryGetComponent<Crate>(out Crate crate)) crate.LoadFromSaveData(crateData);
            
        }

        foreach (PigSaveData pigData in data.pigs)
        {
            GameObject prefabToSpawn = pigPrefab;

            switch ((PigType)pigData.pigType)
            {
                case PigType.Boar:
                    prefabToSpawn = boarPrefab;
                    break;
                case PigType.Devil:
                    prefabToSpawn = devilPrefab;
                    break;
                case PigType.Golden:
                    prefabToSpawn = goldenPigPrefab;
                    break;
                case PigType.Alien:
                    prefabToSpawn = alienPigPrefab;
                    break;
                case PigType.Cyborg:
                    prefabToSpawn = cyborgPigPrefab;
                    break;
                case PigType.Angel:
                    prefabToSpawn = angelPigPrefab;
                    break;
            }

            GameObject pigObj = Instantiate(
                prefabToSpawn,
                new Vector3(pigData.posX, pigData.posY, pigData.posZ),
                new Quaternion(pigData.rotX, pigData.rotY, pigData.rotZ, pigData.rotW)
            );

            if(pigObj.TryGetComponent<Pig>(out Pig pig)) pig.LoadFromSaveData(pigData);
        }

        yield return StartCoroutine(FadeBlackout(1f, 0f));

        yield return StartCoroutine(EnableControls());
        isStarting = false;
    }

    public IEnumerator FadeBlackout(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        Color c = blackout.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            blackout.color = c;

            yield return null;
        }

        c.a = endAlpha;
        blackout.color = c;
    }

    private void SetBlackoutAlpha(float alpha)
    {
        Color c = blackout.color;
        c.a = alpha;
        blackout.color = c;
    }


    IEnumerator EnableControls()
    {
        yield return new WaitForSeconds(0.1f);
        pc.enabled = true;
        pc.canMove = true;
    }

    #region savesystem
    public void SaveNow()
    {
        SaveData data = BuildSaveData();
        SaveSystem.SaveGame(data);
    }

    private SaveData BuildSaveData()
    {
        SaveData data = new();

        data.player = new PlayerSaveData
        {
            posX = player.transform.position.x,
            posY = player.transform.position.y,
            posZ = player.transform.position.z,
            rotX = player.transform.rotation.x,
            rotY = player.transform.rotation.y,
            rotZ = player.transform.rotation.z,
            rotW = player.transform.rotation.w,
            kickStrength = PlayerKick.instance.kickStrength
        };

        data.gameManager = new GameManagerSaveData
        {
            baconCount = GameManager.instance.baconCount,
            baconTotal = GameManager.instance.baconTotal,
            pigsMax = GameManager.instance.pigsMax,
            kickedPigs = GameManager.instance.pigsKicked,
            angelKills = GameManager.instance.angelKills
        };

        if (ShopManager.instance != null)
            data.shop = ShopManager.instance.GetSaveData();

        if (CarrotPatches.instance != null)
        {
            data.carrotPatchInterval = CarrotPatches.instance.normalPatch.spawnInterval;
            data.goldCarrotPatchInterval = CarrotPatches.instance.goldPatch.spawnInterval;
        }

        if (PassiveBacon.instance != null)
            data.passiveBaconInterval = PassiveBacon.instance.spawnInterval;

        Crate[] crates = FindObjectsByType<Crate>(FindObjectsSortMode.None);
        foreach (Crate crate in crates)
            data.crates.Add(crate.GetSaveData());

        Pig[] pigs = FindObjectsByType<Pig>(FindObjectsSortMode.None);
        foreach (Pig pig in pigs)
            data.pigs.Add(pig.GetSaveData());

        return data;
    }

    private void LoadPlayer(PlayerSaveData data)
    {
        if (data == null) return;

        if(player.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.enabled = false; pc.canMove = false;
        }

        player.transform.SetPositionAndRotation(
            new Vector3(data.posX, data.posY, data.posZ),
            new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW)
            );

        PlayerKick.instance.kickStrength = data.kickStrength;
        
    }

    private void LoadGameManager(GameManagerSaveData data)
    {
        if (data == null) return;

        GameManager.instance.baconCount = data.baconCount;
        GameManager.instance.baconTotal = data.baconTotal;
        GameManager.instance.pigsMax = data.pigsMax;
        GameManager.instance.pigsKicked = data.kickedPigs;
        GameManager.instance.angelKills = data.angelKills;

        GameManager.instance.UpdatePigCount();
        ShopManager.instance.UpdateUI();
    }
    #endregion

    private void ClearRuntimeObjects()
    {
        Crate[] crates = FindObjectsByType<Crate>(FindObjectsSortMode.None);
        foreach (Crate c in crates)
            Destroy(c.gameObject);

        Pig[] pigs = FindObjectsByType<Pig>(FindObjectsSortMode.None);
        foreach (Pig p in pigs)
            Destroy(p.gameObject);
    }

    private void StartGameplay()
    {
        BaconCounter.SetActive(true);
        fps.SetActive(true);
        mainmenu.SetActive(false);
        PassiveBacon.instance.running = true;
        LoopingMusic.instance.FadeToSong(2);
    }
    public void QuitGame()
    {
        Debug.Log("Quit pressed");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator SpawnNGPlusPotatoes(int potatoes)
    {
        int spawned = 0;
        while (spawned < potatoes)
        {
            GameObject potato =
            Instantiate(potatoPrefab, potatopos, Quaternion.identity);

            Rigidbody rb = potato.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddTorque(
                    Random.insideUnitSphere * 0.1f,
                    ForceMode.Impulse
                );
            }

            spawned++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnApplicationQuit()
    {
        if(!DayCycle.instance.bossFight && ShopManager.instance.playing == true)
            SaveNow();
    }
}
