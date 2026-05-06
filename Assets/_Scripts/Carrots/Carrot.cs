using UnityEngine;

public class Carrot : MonoBehaviour
{
    private void Start()
    {
        GameManager.instance.carrotCount++;
    }
    private void OnDestroy()
    {
       GameManager.instance.carrotCount--;
        if (CarrotPatches.instance != null)
        {
            CarrotPatches.instance.ClearCarrot(gameObject);
        }
    }
}
