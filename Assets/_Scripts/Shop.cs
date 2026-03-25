using UnityEngine;

public class Shop : MonoBehaviour
{
    public void BuyMoveSpeed()
    {
        if (GameManager.instance.SpendBacon(10))
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().moveSpeed += 1f;
        }
    }

    public void BuyPig()
    {
        if (GameManager.instance.SpendBacon(20))
        {
            Instantiate(Resources.Load("PigPrefab"));
        }
    }
}
