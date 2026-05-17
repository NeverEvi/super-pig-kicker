using UnityEngine;

public class ButtonFlipSound : MonoBehaviour
{
    public AudioClip flip;
    public void PlaySound() 
    {
        AudioHelper.PlayClipAtPosition(
                flip,
                ShopManager.instance.transform.position,
                1f,
                Random.Range(0.9f, 1.1f));
    }
}
