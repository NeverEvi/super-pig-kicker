using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject BaconCounter;
    public GameObject fps;
    public GameObject mainmenu;
    public GameObject spawnPoint;
    public GameObject UI_PANEL;

    [Header("Managers")]
    public ShopManager shopManager;

    [Header("Prefabs")]
    public GameObject cratePrefab;
    public GameObject pigPrefab;
    public GameObject boarPrefab;
    public GameObject goldenPigPrefab;
    public GameObject alienPigPrefab;
    public GameObject cyborgPigPrefab;
    public GameObject angelPigPrefab;
    public GameObject devilPrefab;

    public Vector3 spawnpos = new (1.05110359f, 10.8000002f, 2.64636302f);


    public void NewGame()
    {
        SaveSystem.DeleteSave();

        StartGameplay();

        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerController>().canMove = false;
        BaconCounter.SetActive(true);
        fps.SetActive(true);

        Debug.Log(spawnPoint.transform.position);
        player.transform.position = spawnPoint.transform.position;
        player.transform.rotation = new Quaternion(0f, -0.449043423f, 0f, 0.893509984f);
        mainmenu.SetActive(false);
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerController>().canMove = true;


        Instantiate(cratePrefab, spawnpos, Quaternion.identity);
    }
    public void Continue()
    {
        SaveData data = SaveSystem.LoadGame();
        if (data == null)
        {
            NewGame();
            return;
        }

        StartGameplay();
        ClearRuntimeObjects();

        LoadPlayer(data.player);
        LoadGameManager(data.gameManager);

        if (shopManager != null)
            shopManager.LoadFromSaveData(data.shop);

        if (CarrotPatch.instance != null)
            CarrotPatch.instance.spawnInterval = data.carrotPatchInterval;

        if (CarrotPatchGold.instance != null)
            CarrotPatchGold.instance.spawnInterval = data.goldCarrotPatchInterval;

        if (PassiveBacon.instance != null)
            PassiveBacon.instance.spawnInterval = data.passiveBaconInterval;

        foreach (CrateSaveData crateData in data.crates)
        {
            GameObject crateObj = Instantiate(
                cratePrefab,
                new Vector3(crateData.posX, crateData.posY, crateData.posZ),
                new Quaternion(crateData.rotX, crateData.rotY, crateData.rotZ, crateData.rotW)
            );

            Crate crate = crateObj.GetComponent<Crate>();
            if (crate != null)
                crate.LoadFromSaveData(crateData);
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

            Pig pig = pigObj.GetComponent<Pig>();
            if (pig != null)
                pig.LoadFromSaveData(pigData);
        }
    }
    public void SaveNow()
    {
        SaveData data = BuildSaveData();
        SaveSystem.SaveGame(data);
    }

    private SaveData BuildSaveData()
    {
        SaveData data = new SaveData();

        data.player = new PlayerSaveData
        {
            posX = player.transform.position.x,
            posY = player.transform.position.y,
            posZ = player.transform.position.z,
            rotX = player.transform.rotation.x,
            rotY = player.transform.rotation.y,
            rotZ = player.transform.rotation.z,
            rotW = player.transform.rotation.w
        };

        data.gameManager = new GameManagerSaveData
        {
            baconCount = GameManager.instance.baconCount,
            baconTotal = GameManager.instance.baconTotal,
            pigsMax = GameManager.instance.pigsMax,
            kickedPigs = GameManager.instance.pigsKicked,
            angelKills = GameManager.instance.angelKills
        };

        if (shopManager != null)
            data.shop = shopManager.GetSaveData();

        if (CarrotPatch.instance != null)
            data.carrotPatchInterval = CarrotPatch.instance.spawnInterval;

        if (CarrotPatchGold.instance != null)
            data.goldCarrotPatchInterval = CarrotPatchGold.instance.spawnInterval;

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

        PlayerController pc = player.GetComponent<PlayerController>();
        pc.enabled = false;

        player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
        player.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

        pc.enabled = true;
        pc.canMove = true;
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
        shopManager.UpdateBaconUI();
    }

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
    }

    private void OnApplicationQuit()
    {
        SaveNow();
    }
}
