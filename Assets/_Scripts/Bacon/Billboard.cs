using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (playerCamera == null)
            return;

        transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
                         playerCamera.transform.rotation * Vector3.up);
    }
}