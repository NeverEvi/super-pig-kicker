using UnityEngine;

public class SwitchTabScale : MonoBehaviour
{
    public GameObject tab1;
    public GameObject tab2;
    public AudioClip flip;

    public void SwitchTab(int tabNumber)
    {
        AudioHelper.PlayClipAtPosition(
            flip,
            transform.position,
            0.5f,
            Random.Range(0.9f, 1.1f));

        switch (tabNumber)
        {
            case 1:
                tab1.transform.localScale = new(1f, 1f, 1f);
                tab2.transform.localScale = new(0.9f, 0.9f, 0.9f);
                break;
            case 2:
                tab2.transform.localScale = new(1f, 1f, 1f);
                tab1.transform.localScale = new(0.9f, 0.9f, 0.9f);
                break;
        }
    }
}
