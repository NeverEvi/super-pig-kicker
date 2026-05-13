using UnityEngine;

public class NewGamePlus : MonoBehaviour
{
    public static NewGamePlus instance;

    private void Awake()
    {
        instance = this;
    }
    
}
