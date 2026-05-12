using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public TextMeshProUGUI baconText; // currency display
    public TextMeshProUGUI baconTotalText;
    public bool goldenPigBought = false, cyborgBought = false;
    public bool playing = false;
    public bool deliver = false;

    [Header("Other Prefabs")]
    public GameObject cratePrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip failSound, buySound;

    [Header("Other Pigs")]
    public GameObject CyborgButton, CyboargButton, AngelButton, DevilButton;
    public TextMeshProUGUI DevilCostText;


    [Header("Carrot Patch Upgrade")]
    public GameObject patchButton;
    public TextMeshProUGUI patchCostText;
    public TextMeshProUGUI patchAmountText;
    public int patchCost = 20;        // starting cost
    readonly private float patchUpgradeAmount = 1f;

    public GameObject goldPatchButton;
    public TextMeshProUGUI goldPatchCostText;
    public TextMeshProUGUI goldPatchAmountText;
    public GameObject goldPatch;
    public int goldPatchCost = 50;        // starting cost
    readonly private float goldPatchUpgradeAmount = 1f;

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

    public GameObject Sucko;
    public Button suckoButton;
    public TextMeshProUGUI suckoCostText;
    public bool suckoBought = false;

    [Header("Permits")]
    public Button permitButton;
    public TextMeshProUGUI permitCostText;
    public TextMeshProUGUI permitAmountText;
    public int permitCost = 1000;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject escPanel;
    public bool isOpen = false;
    public bool isEsc = false;

    [Header("Kick Upgrade")]
    public Button kickButton;
    public TextMeshProUGUI kickCostText;
    public TextMeshProUGUI kickAmountText;
    public TextMeshProUGUI kickUI;
    public int kickCost = 200;        // starting cost

    [Header("Trough Upgrade")]
    public Button troughButton;
    public TextMeshProUGUI troughCostText;
    public TextMeshProUGUI troughAmountText;
    public int troughCost = 10;        // starting cost
    readonly private float troughUpgradeAmount = 0.5f;

    public enum UpgradeWhat 
    { 
        Trough,
        Patch, GoldPatch,
        Permit,
        Dish, Tech,
        Kick
    }
    public UpgradeWhat what;

    void Awake() => instance = this;
    void Start()
    {
        shopPanel.SetActive(false);
        UpdateUI();
    }
    void Update()
    {
        if (playing)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!isEsc) ToggleShop();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isOpen) ToggleShop();
                else ToggleEsc();
            }
        }
    }

    public void Unlock(GameObject button, string unlockName, bool setInteractable=false, bool displayPopup=true, GameObject secondary=null)
    {
        if (!setInteractable) button.SetActive(true);
        else button.GetComponent<Button>().interactable = true;
        if (secondary != null) secondary.SetActive(true); 
        if (displayPopup) PopupManager.instance.DisplayPopup(button.GetComponent<Image>().sprite, $"Unlocked {unlockName}");
    }

    public void BuyItem(int cost)
    {
        if (GameManager.instance.baconCount >= cost)
        {
            string itemName = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
            Vector3 spawnpos = new (Random.Range(-2f, 6f), Random.Range(4f, 5.5f), Random.Range(-2f, 4f));
            
            if(TryBuyPig(itemName, spawnpos)) 
            {
                GameManager.instance.baconCount -= cost;
                UpdateUI();
                PlayBuySound();
                return;
            }
            PlayFailSound();
            return;
        }
        PlayFailSound();
    }
    private bool TryBuyPig(string itemName, Vector3 spawnpos)
    {
        int pigs = GameManager.instance.pigsCount;
        int maxPigs = GameManager.instance.pigsMax;
        int crates = GameManager.instance.crateCount;

        if (pigs < maxPigs && crates < 5)
        {
            if(itemName != "DEVIL")
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
                            GameManager.instance.SPK += 0.01f;
                            Unlock(patchButton, "Carrot Patch");
                        }
                        break;
                    case "ALIEN":
                        crate.pigType = PigType.Alien;
                        if (!alienBought)
                        {
                            alienBought = true;
                            GameManager.instance.SSK += 0.01f;
                            Unlock(techButton, "UFO Technology", false, true, UFO);
                            if(StarFall.instance != null && !StarFall.instance.enabled)
                            {
                                StarFall.instance.enabled = true;
                            }
                        }
                        break;
                    case "CYBORG": //Buying first Cyborg unlocks: CyBOARg and Gold carrot patch.
                        crate.pigType = PigType.Cyborg; 
                        if (!cyborgBought)
                        {
                            cyborgBought = true;
                            GameManager.instance.SPK += 0.01f;
                            Unlock(goldPatchButton, "Gold Carrot Patch",false,true, goldPatch);
                            Unlock(CyboargButton, "CyBOARg");
                        }
                        break;
                    case "CYBOARG": crate.pigType = PigType.Cyboarg; break;
                    case "ANGEL": crate.pigType = PigType.Angel; break;
                }
            }
            else
            {
                if (!DayCycle.instance.bossFight)
                {
                    ToggleShop();
                    Summoning.instance.SUMMON();
                    DevilButton.GetComponent<Button>().interactable = false;
                }

            }
            return true;
        }
        return false;
    }
    #region TECH
    public void BuyDish() //Buy the Satellite dish to unlock Aliens
    {
        if (GameManager.instance.baconCount >= 800 && Dish.activeSelf == false)
        {
            Unlock(AlienButton, "Alien Pigs", false, true, Dish);
            GameManager.instance.baconCount -= 800;
            GameManager.instance.SPK += 0.01f;
            dishButton.GetComponent<Button>().interactable = false;
            dishCostText.text = "Satellite Dish: MAX";
            PlayBuySound();
            UpdateUI();
        }
        else PlayFailSound();
    }
    public void BuyTech() //Disassemble the UFO to unlock Cyborgs
    {
        if (GameManager.instance.baconCount >= 2000 && hasTech == false)
        {
            hasTech = true;
            Unlock(CyborgButton, "Cyborg Pigs");
            GameManager.instance.baconCount -= 2000;
            techButton.GetComponent<Button>().interactable = false;
            techCostText.text = "Disassemble Ship: MAX";
            UFO.SetActive(false);
            PlayBuySound();
            UpdateUI();
        }
        else PlayFailSound();
    }

    public void BuySucker() //Buy the Satellite dish to unlock Aliens
    {
        if (GameManager.instance.baconCount >= 1200 && Sucko.activeSelf == false)
        {
            GameManager.instance.baconCount -= 1200;
            Sucko.SetActive(true);
            suckoBought = true;
            suckoButton.GetComponent<Button>().interactable = false;
            suckoCostText.text = "Suck-o 3000: MAX";
            PlayBuySound();
            UpdateUI();
        }
        else PlayFailSound();
    }
    #endregion

    #region PERMIT
    public void BuyPermit() //Buy pig permits
    {
        int maxPigs = GameManager.instance.pigsMax;

        if (GameManager.instance.baconCount >= permitCost && maxPigs < 10)
        {
            
            GameManager.instance.baconCount -= permitCost;
            
            UpgradePermit();
            PlayBuySound();
            UpdateUI();
            GameManager.instance.UpdatePigCount();
        }
        else PlayFailSound();
    }
    private void UpgradePermit()
    {
        GameManager.instance.pigsMax++;

        permitCost = Mathf.RoundToInt(1.45f * permitCost);
        GameManager.instance.SPK += 0.002f;
        GameManager.instance.SSK += 0.001f;
        if (GameManager.instance.pigsMax >= 10)
        {
            permitCostText.text = "Pig Permit: MAX";
            permitButton.interactable = false;
        }
        else
            permitCostText.text = "Pig Permit: " + permitCost + " bacon";
    }
    #endregion

    #region TROUGH
    public void BuyTrough()
    {
        if (GameManager.instance.baconCount >= troughCost 
            && PassiveBacon.instance.spawnInterval > 2f)
        {
            GameManager.instance.baconCount -= troughCost;
            UpgradeTrough();
            PlayBuySound();
            UpdateUI();
        }
        else PlayFailSound();
    }
    private void UpgradeTrough()
    {
        PassiveBacon.instance.spawnInterval = Mathf.Max(2f, PassiveBacon.instance.spawnInterval - troughUpgradeAmount);

        troughCost = Mathf.RoundToInt(1.3f*troughCost);
        if (PassiveBacon.instance.spawnInterval <= 2f && troughButton != null)
        {
            troughButton.interactable = false;
        }
    }
    #endregion

    #region KICK
    public void BuyKick()
    {
        if (GameManager.instance.baconCount >= kickCost
            && PlayerKick.instance.kickStrength<20)
        {
            GameManager.instance.baconCount -= kickCost;
            UpgradeKick();
            PlayBuySound();
            UpdateUI();
        }
        else PlayFailSound();
    }
    private void UpgradeKick()
    {
        PlayerKick.instance.kickStrength++;
        GameManager.instance.SPK += 0.005f;
        GameManager.instance.SSK += 0.002f;
        kickCost = Mathf.RoundToInt(1.53f * kickCost);
        if (PlayerKick.instance.kickStrength>=20)
        {
            kickButton.interactable = false;
        }
    }

    #endregion

    #region CARROTS AND PATCHES


    public void BuyPatch(bool gold = false)
    {
        if (CarrotPatches.instance == null) { PlayFailSound();return; }

        if (!gold)
        {
            if (GameManager.instance.baconCount >= patchCost &&
                CarrotPatches.instance.normalPatch.spawnInterval > 10f)
            {
                GameManager.instance.baconCount -= patchCost;
                UpgradePatch(false);
                
                PlayBuySound();
                UpdateUI();
                return;
            }
            PlayFailSound();
            return;
        }
        if (GameManager.instance.baconCount >= goldPatchCost &&
                CarrotPatches.instance.goldPatch.spawnInterval > 10f)
        {
            GameManager.instance.baconCount -= goldPatchCost;
            UpgradePatch(true);
                
            PlayBuySound();
            UpdateUI();
            return;
        }
        PlayFailSound();
    }
    private void UpgradePatch(bool gold = false)
    {
        if (CarrotPatches.instance == null) return;

        if (!gold)
        {
            float progress = Mathf.InverseLerp(60f, 10f, CarrotPatches.instance.normalPatch.spawnInterval);
            float multiplier = Mathf.Lerp(1.2f, 1.3f, progress);

            CarrotPatches.instance.normalPatch.spawnInterval =
                Mathf.Max(10f, CarrotPatches.instance.normalPatch.spawnInterval - patchUpgradeAmount);

            patchCost = Mathf.RoundToInt(multiplier * patchCost);

            if (CarrotPatches.instance.normalPatch.spawnInterval <= 10f && patchButton != null)
            {
                patchButton.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            float progress = Mathf.InverseLerp(60f, 10f, CarrotPatches.instance.goldPatch.spawnInterval);
            float multiplier = Mathf.Lerp(1.22f, 1.45f, progress);

            CarrotPatches.instance.goldPatch.spawnInterval =
                Mathf.Max(10f, CarrotPatches.instance.goldPatch.spawnInterval - goldPatchUpgradeAmount);

            goldPatchCost = Mathf.RoundToInt(multiplier * goldPatchCost);

            if (CarrotPatches.instance.goldPatch.spawnInterval <= 10f && goldPatchButton != null)
            {
                goldPatchButton.GetComponent<Button>().interactable = false;
            }
        }
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
    public void ToggleEsc()
    {
        isEsc = !isEsc;
        escPanel.SetActive(isEsc);
        if (!isEsc) CreditsManager.instance.CloseCredits();
        UpdatePauseAndCursor();
    }
    public void ToggleShop()
    {
        if (!isOpen && Summoning.instance.isSummoning) return;
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (!AngelButton.activeSelf && GameManager.instance.baconTotal >= 25000) Unlock(AngelButton, "Angel Pig");
        UpdateUI();
        UpdatePauseAndCursor();
    }
    public void UpdatePauseAndCursor()
    {
        bool menuOpen = isEsc || isOpen;

        Time.timeScale = menuOpen ? 0f : 1f;
        ToggleCursor(menuOpen);
    }    
    public void ToggleCursor(bool cur)
    {
        if(cur)
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

    #region UI
    public void UpdateUI()
    {
        UpdateBaconUI();
        UpdateTroughUI();
        UpdatePatchUI();
        UpdateKickUI();
        UpdateAmounts();
    }
    public void UpdateBaconUI()
    {
        baconText.text = "Bacon: " + GameManager.instance.baconCount;
        baconTotalText.text = "Total Bacon: " + GameManager.instance.baconTotal;

    }
    private void UpdateKickUI()
    {
        if(PlayerKick.instance.kickStrength >=20)
        {
            kickCostText.text = $"+Kick Strength: MAX";
            return;
        }
        kickCostText.text = $"+Kick Strength: {kickCost} bacon";
        kickUI.text = $"Kick Strength: {PlayerKick.instance.kickStrength}";
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
    private void UpdatePatchUI()
    {
        if (CarrotPatches.instance.normalPatch.spawnInterval <= 10f)
        {
            patchCostText.text = "+Patch: MAX";
        }
        else
            patchCostText.text = "+Patch: " + patchCost + " bacon";

        if (CarrotPatches.instance.goldPatch.spawnInterval <= 10f)
        {
            goldPatchCostText.text = "+Gold Patch: MAX";
        }
        else
            goldPatchCostText.text = "+Gold Patch: " + goldPatchCost + " bacon";
        
    }
    public void UpdateAmounts()
    {
        // TROUGH
        float troughStart = 30f;
        float troughMin = 2f;
        float troughStep = 0.5f;

        int troughMax = Mathf.RoundToInt((troughStart - troughMin) / troughStep);
        int troughCurrent = Mathf.RoundToInt((troughStart - PassiveBacon.instance.spawnInterval) / troughStep);

        troughAmountText.text = $"{troughCurrent}/{troughMax}";


        // NORMAL PATCH
        float patchStart = 60f;
        float patchMin = 10f;
        float patchStep = 1f;

        int patchMax = Mathf.RoundToInt((patchStart - patchMin) / patchStep);
        int patchCurrent = Mathf.RoundToInt(
            (patchStart - CarrotPatches.instance.normalPatch.spawnInterval) / patchStep);

        patchAmountText.text = $"{patchCurrent}/{patchMax}";


        // GOLD PATCH
        if (goldPatchButton.activeSelf)
        {
            int goldPatchCurrent = Mathf.RoundToInt(
                (patchStart - CarrotPatches.instance.goldPatch.spawnInterval) / patchStep);

            goldPatchAmountText.text = $"{goldPatchCurrent}/{patchMax}";
        }
        
        permitAmountText.text = $"{GameManager.instance.pigsMax}/10";
        kickAmountText.text = $"{PlayerKick.instance.kickStrength}/20";
    }
    #endregion

    #region SaveData
    public ShopSaveData GetSaveData()
    {
        return new ShopSaveData
        {
            goldenPigBought = goldenPigBought,
            alienBought = alienBought,
            cyborgBought = cyborgBought,
            hasTech = hasTech,
            suckoBought = suckoBought,

            permitCost = permitCost,
            troughCost = troughCost,
            patchCost = patchCost,
            goldPatchCost = goldPatchCost,
            kickCost = kickCost,

            devilButtonActive = DevilButton != null && DevilButton.activeSelf,
            devilCostText = DevilCostText.text,
        };
    }

    public void LoadFromSaveData(ShopSaveData data)
    {
        if (data == null) return;

        goldenPigBought = data.goldenPigBought;
        alienBought = data.alienBought;
        cyborgBought = data.cyborgBought;
        hasTech = data.hasTech;
        suckoBought = data.suckoBought;

        permitCost = data.permitCost;
        troughCost = data.troughCost;
        patchCost = data.patchCost;
        goldPatchCost = data.goldPatchCost;
        kickCost = data.kickCost;

        RefreshUnlockStates();

        if (DevilButton != null) 
        { 
            DevilButton.SetActive(data.devilButtonActive);
            DevilCostText.text = data.devilCostText;
        }

        RefreshButtonStates();

        UpdateUI();
    }

    private void RefreshUnlockStates()
    {

        if (patchButton != null)
            patchButton.SetActive(goldenPigBought);

        if (Dish != null)
            Dish.SetActive(alienBought || hasTech);

        if (AlienButton != null)
            AlienButton.SetActive(Dish != null && Dish.activeSelf);

        if (UFO != null)
            UFO.SetActive(alienBought && !hasTech);

        if (techButton != null)
            techButton.SetActive(alienBought);

        if (CyborgButton != null)
            CyborgButton.SetActive(hasTech);

        if (Sucko != null)
            Sucko.SetActive(suckoBought);

        if (goldPatchButton != null)
            goldPatchButton.SetActive(cyborgBought);

        if (goldPatch != null)
            goldPatch.SetActive(cyborgBought);

        if (CyboargButton != null)
            CyboargButton.SetActive(cyborgBought);

        if (AngelButton != null && GameManager.instance.baconTotal >= 5000)
            AngelButton.SetActive(true);
    }
    private void RefreshButtonStates()
    {
        if (dishButton != null)
            dishButton.interactable = !hasTech && (Dish == null || !Dish.activeSelf);

        if (permitButton != null)
            permitButton.interactable = GameManager.instance.pigsMax < 10;

        if (troughButton != null && PassiveBacon.instance != null)
            troughButton.interactable = PassiveBacon.instance.spawnInterval > 2f;

        if (patchButton != null && CarrotPatches.instance != null)
            patchButton.GetComponent<Button>().interactable =
                CarrotPatches.instance.normalPatch.spawnInterval > 10f;

        if (goldPatchButton != null && CarrotPatches.instance != null)
            goldPatchButton.GetComponent<Button>().interactable =
                CarrotPatches.instance.goldPatch.spawnInterval > 10f;

        if (kickButton != null)
            kickButton.interactable = PlayerKick.instance.kickStrength < 20;

        if (suckoButton != null)
            suckoButton.interactable = !suckoBought;
    }

    #endregion
}