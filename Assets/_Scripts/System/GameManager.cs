using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int baconCount = 0;
    public int baconTotal = 0;

    public int pigsCount=0;
    public int pigsMax = 3;
    public int carrotCount = 0;
    public int crateCount = 0;

    public int pigsKicked = 0;
    public int angelKills = 0;
    public float SSK = 0.05f;
    public float SPK = 0.05f;

    public bool immune = false;
    public int totalScore = 0;

    public int newGamePlus = 0;
    public TextMeshProUGUI baconText;
    public TextMeshProUGUI pigKickedText;
    public TextMeshProUGUI pigCountText;
    public TextMeshProUGUI ScoreText;

    void Awake() => instance = this;

    public void AddBacon(int amount)
    {
        baconCount += amount; baconTotal += amount;
        UpdateScore(amount);
        ShopManager.instance.UpdateBaconUI();
    }

    public bool SpendBacon(int cost)
    {
        if (baconCount >= cost)
        {
            baconCount -= cost;
            ShopManager.instance.UpdateBaconUI();
            return true;
        }
        return false;
    }
    public void UpdateKickedPigs()
    {
        pigsKicked++;
        UpdateScore(1);
        pigKickedText.text = ShopManager.instance.L("pigs_kicked", pigsKicked);
    }    
    public void UpdatePigCount()
    {
        pigCountText.text = ShopManager.instance.L("pigs_owned", pigsCount, pigsMax);
    }   
    public void UpdateScore(int points)
    {
        totalScore += points;
        ScoreText.text = $"Score: {totalScore}";
    }
}
