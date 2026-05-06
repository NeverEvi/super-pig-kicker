using UnityEngine;

public static class AudioHelper
{ 
        public static void PlayClipAtPosition(
            AudioClip clip, 
            Vector3 position, 
            float volume = 1f, 
            float pitch = 1f)
        {
            if (clip == null) return;

            GameObject temp = new("TempAudio");
            temp.transform.position = position;
        
            AudioSource source = temp.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.spatialBlend = 1f;
            source.Play();

            Object.Destroy(temp, clip.length / Mathf.Abs(pitch));
        }
    
}
