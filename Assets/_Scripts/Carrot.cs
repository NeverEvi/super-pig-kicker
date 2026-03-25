using UnityEngine;

public class Carrot : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.Log("Destroying Carrot");
        if (CarrotPatch.instance != null && CarrotPatch.instance.currentCarrot==this.gameObject)
        {
            CarrotPatch.instance.ClearCarrot();
            Debug.Log("Carrot Cleared");
        }
        else if (CarrotPatchGold.instance != null && CarrotPatchGold.instance.currentCarrot == this.gameObject)
        {
            CarrotPatchGold.instance.ClearCarrot();
            Debug.Log("Gold Carrot Cleared");
        }
    }
}
