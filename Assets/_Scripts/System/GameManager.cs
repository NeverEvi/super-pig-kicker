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

    public bool immune = false;

    public TextMeshProUGUI baconText;
    public TextMeshProUGUI pigKickedText;
    public TextMeshProUGUI pigCountText;

    void Awake() => instance = this;

    public void AddBacon(int amount)
    {
        baconCount += amount; baconTotal += amount;
        baconText.text = "Bacon: " + baconCount;
    }

    public bool SpendBacon(int cost)
    {
        if (baconCount >= cost)
        {
            baconCount -= cost;
            baconText.text = "Bacon: " + baconCount;
            return true;
        }
        return false;
    }
    public void UpdateKickedPigs()
    {
        pigsKicked++;
        pigKickedText.text = "Pigs Kicked: " + pigsKicked;
    }    
    public void UpdatePigCount()
    {
        pigCountText.text = "Pigs Owned: " + pigsCount + "/" + pigsMax;
    }   
}
