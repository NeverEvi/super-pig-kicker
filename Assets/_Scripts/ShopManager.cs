using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public TextMeshProUGUI baconText; // currency display
    public TextMeshProUGUI baconTotalText;
    private bool goldenPigBought = false;
    private bool cyborgBought = false;

    [Header("Other Prefabs")]
    public GameObject cratePrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip failSound, buySound;

    [Header("Other Pigs")]
    public GameObject CyborgButton;
    public GameObject CyboargButton;
    public GameObject AngelButton;
    public GameObject DevilButton;
    public TextMeshProUGUI DevilCostText;

    void Awake() => instance = this;
    void Start()
    {
        shopPanel.SetActive(false);
        UpdateBaconUI(); UpdateTroughUI();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ToggleShop();
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen) ToggleShop();
            else ToggleEsc();
        }
    }

    public void BuyItem(int cost)
    {
        if (GameManager.instance.baconCount >= cost)
        {
            string itemName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
            Vector3 spawnpos = new (Random.Range(-2f, 6f), 4, Random.Range(-2f, 4f));
            if (itemName == "PIG" || itemName == "BOAR" || itemName == "GOLDPIG" 
                || itemName == "ALIEN" || itemName == "CYBORG" || itemName == "ANGEL")
            {
                if(TryBuyPig(itemName, spawnpos)) 
                {
                    GameManager.instance.baconCount -= cost;
                    UpdateBaconUI();
                }
                return;
            }
            else if (itemName == "CARROT")
            {
                Instantiate(carrotPrefab, spawnpos, Quaternion.identity);
                GameManager.instance.baconCount -= cost;
                UpdateBaconUI();
            }
            PlayBuySound();
        }
        else PlayFailSound();
    }
    private bool TryBuyPig(string itemName, Vector3 spawnpos)
    {
        int pigs = GameManager.instance.pigsCount;
        int maxPigs = GameManager.instance.pigsMax;
        int crates = GameManager.instance.crateCount;

        if (pigs < maxPigs && crates < 5)
        {
            GameObject pigCrate = Instantiate(cratePrefab, spawnpos, Quaternion.identity);
            Crate crate = pigCrate.GetComponent<Crate>();
            switch (itemName)
            {
                case "PIG": crate.pigType = PigType.Pig; break;
                case "BOAR": crate.pigType = PigType.Boar; break;
                case "GOLDPIG": 
                    crate.pigType = PigType.Golden;
                    if (!goldenPigBought)
                    {
                        goldenPigBought = true;
                        if (carrotButton != null)
                            carrotButton.SetActive(true);
                        if (patchButton != null)
                            patchButton.SetActive(true);
                    }
                    break;
                case "ALIEN":
                    crate.pigType = PigType.Alien; 
                    if(!alienBought)
                    {
                        alienBought = true;
                        UFO.SetActive(true);
                        techButton.SetActive(true);
                    }
                    break;
                case "CYBORG": 
                    crate.pigType = PigType.Cyborg;
                    if (!cyborgBought)
                    {
                        cyborgBought = true;
                        goldPatchButton.SetActive(true);
                        CyboargButton.SetActive(true);
                    }

                    break;
                case "ANGEL": crate.pigType = PigType.Angel; break;
            }
            PlayBuySound();
            return true;
        }
        else PlayFailSound();
        return false;
    }
    #region TECH
    [Header("Tech Tree")]
    public GameObject Dish;
    public Button dishButton;
    public TextMeshProUGUI dishCostText;
    public GameObject AlienButton;
    public bool alienBought = false;
    public GameObject UFO;
    public GameObject techButton;
    public TextMeshProUGUI techCostText;
    public bool hasTech = false;

    public void BuyDish()
    {
        if (GameManager.instance.baconCount >= 800 && Dish.activeSelf == false)
        {
            Dish.SetActive(true);
            AlienButton.SetActive(true);
            GameManager.instance.baconCount -= 800;
            dishButton.GetComponent<Button>().interactable = false;
            dishCostText.text = "Satellite Dish - MAX";
            PlayBuySound();
            UpdateBaconUI();
        }
        else PlayFailSound();
    }
    public void BuyTech()
    {
        if (GameManager.instance.baconCount >= 2000 && hasTech == false)
        {
            hasTech = true;
            CyborgButton.SetActive(true);
            GameManager.instance.baconCount -= 2000;
            techButton.GetComponent<Button>().interactable = false;
            techCostText.text = "Disassemble Ship - MAX";
            UFO.SetActive(false);
            PlayBuySound();
            UpdateBaconUI();
        }
        else PlayFailSound();
    }
    #endregion

    #region PERMIT
    [Header("Permits")]
    public Button permitButton;
    public TextMeshProUGUI permitCostText;
    public int permitCost = 1000;

    public void BuyPermit()
    {
        int maxPigs = GameManager.instance.pigsMax;

        if (GameManager.instance.baconCount >= permitCost && maxPigs < 10)
        {
            
            GameManager.instance.baconCount -= permitCost;
            UpgradePermit();
            PlayBuySound();
            UpdateBaconUI();
            GameManager.instance.UpdatePigCount();
        }
        else PlayFailSound();
    }
    private void UpgradePermit()
    {
        GameManager.instance.pigsMax++;

        permitCost = Mathf.RoundToInt(1.5f * permitCost);
        if (GameManager.instance.pigsMax >= 10)
        {
            permitCostText.text = "Pig Permit - MAX";
            permitButton.interactable = false;
        }
        else
            permitCostText.text = "Pig Permit - " + permitCost + " bacon";
    }
    #endregion

    #region TROUGH
    [Header("Trough Upgrade")]
    public Button troughButton;
    public TextMeshProUGUI troughCostText;
    public int troughCost = 10;        // starting cost
    readonly private float troughUpgradeAmount = 0.5f;

    public void BuyTrough()
    {
        if (PassiveBacon.instance.spawnInterval <= 2f)
        {
            PlayFailSound();
            return;
        }
        if (GameManager.instance.baconCount >= troughCost)
        {
            UpgradeTrough();
            GameManager.instance.baconCount -= troughCost;
            PlayBuySound();
            UpdateBaconUI();
        }
        else PlayFailSound();
    }
    private void UpgradeTrough()
    {
        PassiveBacon.instance.spawnInterval = Mathf.Max(2f, PassiveBacon.instance.spawnInterval - troughUpgradeAmount);

        troughCost *= 2;
        UpdateTroughUI();
        if (PassiveBacon.instance.spawnInterval <= 2f && troughButton != null)
        {
            troughButton.interactable = false;
        }
    }

    private void UpdateTroughUI()
    {
        if (PassiveBacon.instance.spawnInterval <= 2f)
        {
            troughCostText.text = "+Trough: MAX";
        }
        else
            troughCostText.text = "+Trough: " + troughCost + " bacon";
    }

    #endregion

    #region CARROTS AND PATCHES
    [Header("Carrot")]
    public GameObject carrotPrefab;
    public GameObject carrotButton;
    public GameObject goldCarrotPrefab;
    public GameObject goldCarrotButton;

    [Header("Carrot Patch Upgrade")]
    public GameObject patchButton;
    public TextMeshProUGUI patchCostText;
    public int patchCost = 20;        // starting cost
    readonly private float patchUpgradeAmount = 1f;
    public GameObject goldPatchButton;
    public TextMeshProUGUI goldPatchCostText;
    public int goldPatchCost = 50;        // starting cost
    readonly private float goldPatchUpgradeAmount = 1f;

    public void BuyPatch(bool gold = false)
    {
        if (gold==false)
        {
            if (GameManager.instance.baconCount >= patchCost && CarrotPatch.instance.spawnInterval <= 10f)
            {
                UpgradePatch(false);
                GameManager.instance.baconCount -= patchCost;
                PlayBuySound();
                UpdateBaconUI();
            }
            else PlayFailSound();
        }
        else
        {
            if (GameManager.instance.baconCount >= goldPatchCost && CarrotPatchGold.instance.spawnInterval <= 10f)
            {
                UpgradePatch(true);
                GameManager.instance.baconCount -= goldPatchCost;
                PlayBuySound();
                UpdateBaconUI();
            }
            else PlayFailSound();
        }
    }
    private void UpgradePatch(bool gold = false)
    {
        if (gold == false)
        {
            CarrotPatch.instance.spawnInterval = Mathf.Max(10f, CarrotPatch.instance.spawnInterval - patchUpgradeAmount);

            patchCost *= 2;
            UpdatePatchUI();
            if (CarrotPatch.instance.spawnInterval <= 2f && patchButton != null)
            {
                patchButton.GetComponent<Button>().interactable = false;
            }
        }
        else 
        {
            CarrotPatchGold.instance.spawnInterval = Mathf.Max(10f, CarrotPatchGold.instance.spawnInterval - goldPatchUpgradeAmount);

            goldPatchCost = Mathf.RoundToInt(2.2f*goldPatchCost);
            UpdatePatchUI();
            if (CarrotPatchGold.instance.spawnInterval <= 2f && goldPatchButton != null)
            {
                goldPatchButton.GetComponent<Button>().interactable = false;
            }
        }
    }
    private void UpdatePatchUI()
    {
        if (CarrotPatch.instance.spawnInterval <= 10f)
        {
            patchCostText.text = "+Patch: MAX";
        }
        else
            patchCostText.text = "+Patch: " + patchCost + " bacon";

        if (CarrotPatchGold.instance.spawnInterval <= 10f)
        {
            goldPatchCostText.text = "+Gold Patch: MAX";
        }
        else
            goldPatchCostText.text = "+Gold Patch: " + goldPatchCost + " bacon";
    }

    #endregion

    #region AUDIO
    private void PlayBuySound()
    {
        if (audioSource != null && buySound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // tiny pitch shift
            audioSource.PlayOneShot(buySound);
        }
    }   
    private void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // tiny pitch shift
            audioSource.PlayOneShot(failSound);
        }
    }
    #endregion

    #region TOGGLES
    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject escPanel;
    public bool isOpen = false;
    public bool isEsc = false;

    public void ToggleEsc()
    {
        isEsc = !isEsc;
        escPanel.SetActive(isEsc);
    }
    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (!AngelButton.activeSelf && GameManager.instance.baconTotal >= 5000)
        {
            AngelButton.SetActive(true);
            Debug.Log("Angel active");
        }  
        UpdateBaconUI();

        Time.timeScale = isOpen ? 0f : 1f;
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    #endregion
    public void UpdateBaconUI()
    {
        baconText.text = "Bacon: " + GameManager.instance.baconCount;
        baconTotalText.text = "Total Bacon: " + GameManager.instance.baconTotal;

    }

    public ShopSaveData GetSaveData()
    {
        return new ShopSaveData
        {
            goldenPigBought = goldenPigBought,
            alienBought = alienBought,
            hasTech = hasTech,

            permitCost = permitCost,
            troughCost = troughCost,
            patchCost = patchCost,

            dishActive = Dish != null && Dish.activeSelf,
            ufoActive = UFO != null && UFO.activeSelf,
            alienButtonActive = AlienButton != null && AlienButton.activeSelf,
            cyborgButtonActive = CyborgButton != null && CyborgButton.activeSelf,
            cyboargButtonActive = CyboargButton != null && CyboargButton.activeSelf,
            angelButtonActive = AngelButton != null && AngelButton.activeSelf,

            devilButtonActive = DevilButton != null && DevilButton.activeSelf,
            devilCostText = DevilCostText.text,

            carrotButtonActive = carrotButton != null && carrotButton.activeSelf,
            patchButtonActive = patchButton != null && patchButton.activeSelf,
            goldPatchButtonActive = goldPatchButton != null && goldPatchButton.activeSelf,
            techButtonActive = techButton != null && techButton.activeSelf,

            dishButtonInteractable = dishButton != null && dishButton.interactable,
            permitButtonInteractable = permitButton != null && permitButton.interactable,
            troughButtonInteractable = troughButton != null && troughButton.interactable,
            patchButtonInteractable = patchButton != null && patchButton.GetComponent<Button>().interactable,
            goldPatchButtonInteractable = goldPatchButton != null && goldPatchButton.GetComponent<Button>().interactable
        };
    }

    public void LoadFromSaveData(ShopSaveData data)
    {
        if (data == null) return;

        goldenPigBought = data.goldenPigBought;
        alienBought = data.alienBought;
        hasTech = data.hasTech;

        permitCost = data.permitCost;
        troughCost = data.troughCost;
        patchCost = data.patchCost;

        if (Dish != null) Dish.SetActive(data.dishActive);
        if (UFO != null) UFO.SetActive(data.ufoActive);
        if (AlienButton != null) AlienButton.SetActive(data.alienButtonActive);
        if (CyborgButton != null) CyborgButton.SetActive(data.cyborgButtonActive);
        if (CyboargButton != null) CyboargButton.SetActive(data.cyboargButtonActive);
        if (AngelButton != null) AngelButton.SetActive(data.angelButtonActive);
        if (DevilButton != null) 
        { 
            DevilButton.SetActive(data.devilButtonActive);
            DevilCostText.text = data.devilCostText;
        }
        if (carrotButton != null) carrotButton.SetActive(data.carrotButtonActive);
        if (patchButton != null) patchButton.SetActive(data.patchButtonActive);
        if (goldPatchButton != null) goldPatchButton.SetActive(data.goldPatchButtonActive);
        if (techButton != null) techButton.SetActive(data.techButtonActive);

        if (dishButton != null) dishButton.interactable = data.dishButtonInteractable;
        if (permitButton != null) permitButton.interactable = data.permitButtonInteractable;
        if (troughButton != null) troughButton.interactable = data.troughButtonInteractable;
        if (patchButton != null) patchButton.GetComponent<Button>().interactable = data.patchButtonInteractable;
        if (goldPatchButton != null) goldPatchButton.GetComponent<Button>().interactable = data.goldPatchButtonInteractable;

        permitCostText.text = permitButton != null && !permitButton.interactable
            ? "Pig Permit - MAX"
            : "Pig Permit - " + permitCost + " bacon";

        UpdateTroughUI();
        UpdatePatchUI();
        UpdateBaconUI();
    }
}