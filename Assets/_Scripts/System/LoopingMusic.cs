using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopingMusic : MonoBehaviour
{
    public AudioClip introMusicClip;
    public AudioClip musicClip; // assign your track in the inspector
    public AudioClip bossMusicClip;

    private AudioSource audioSource;
    public static LoopingMusic instance;

    private Coroutine fadeRoutine;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (musicClip != null)
        {
            audioSource.clip = introMusicClip;
            audioSource.loop = true;   // loop indefinitely
            audioSource.playOnAwake = true; // optional: start immediately
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No music clip assigned to LoopingMusic script!");
        }
    }

    public void FadeToSong(int song)
    {
        AudioClip targetClip = null;

        switch (song)
        {
            case 1: targetClip = introMusicClip; break;
            case 2: targetClip = musicClip; break;
            case 3: targetClip = bossMusicClip; break;
        }

        if (targetClip == null)
        {
            Debug.LogWarning("Target music clip is null!");
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetClip, 1f));
    }

    private IEnumerator FadeRoutine(AudioClip newClip, float duration)
    {
        float startVolume = audioSource.volume;

        //Fade OUT
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        audioSource.volume = 0f;

        //Swap clip
        audioSource.clip = newClip;
        audioSource.Play();

        //Fade IN
        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / duration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}
