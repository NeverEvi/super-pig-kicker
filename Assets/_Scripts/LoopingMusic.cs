using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopingMusic : MonoBehaviour
{
    public AudioClip musicClip; // assign your track in the inspector
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.loop = true;   // loop indefinitely
            audioSource.playOnAwake = true; // optional: start immediately
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No music clip assigned to LoopingMusic script!");
        }
    }
}
