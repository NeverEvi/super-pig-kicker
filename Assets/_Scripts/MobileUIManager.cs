using UnityEngine;

public class MobileUIManager : MonoBehaviour
{
    public GameObject onScreenControls; // parent of all onscreen buttons/sticks

    void Start()
    {
        bool isMobile = Application.isMobilePlatform;
        onScreenControls.SetActive(isMobile);
    }
}
