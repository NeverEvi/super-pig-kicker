using System.Collections;
using UnityEngine;

public class Star : MonoBehaviour
{
    public ParticleSystem streakParticles, popParticles;
   
    public float streakSpeedThreshold = 0.15f;
    private float jumpT = 0, jumpTimer = 5f;
    private Rigidbody rb;
    private bool kicked = false;
    public AudioClip[] twinkleClips; // drag your 2 clips here
    public AudioClip popClip;
    private AudioSource twinkleAudio;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        twinkleAudio = GetComponent<AudioSource>();
        twinkleAudio.loop = false; // IMPORTANT: we’ll handle looping manually
        twinkleAudio.playOnAwake = false;
        // Lock X rotation so it can’t tip onto its face that way
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
    }

    void FixedUpdate()
    {
        bool moving = rb.linearVelocity.magnitude > streakSpeedThreshold;
        Vector3 euler = rb.rotation.eulerAngles;
        rb.MoveRotation(Quaternion.Euler(0f, euler.y, euler.z));
        if (!moving)
        {
            jumpT += Time.deltaTime;
            if (jumpT >= jumpTimer)
            {
                jumpT = 0;
                jumpTimer = Random.Range(3.5f, 12f);
                if (TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    // Slight jump
                    Vector3 jump = new (
                        Random.Range(-0.5f, 0.5f),
                        Random.Range(0.5f, 0.8f),
                        Random.Range(-0.5f, 0.5f)
                    );

                    rb.AddForce(jump * Random.Range(0.5f, 0.7f), ForceMode.Impulse);

                    // Add spin
                    Vector3 spin = new (
                        0f,
                        Random.Range(-2f, 2f),
                        Random.Range(-2f, 2f)
                    );

                    rb.AddTorque(spin, ForceMode.Impulse);
                }
            }
        }

        if (moving)
        {
            if (!streakParticles.isPlaying)
                streakParticles.Play();
            if (!twinkleAudio.isPlaying)
            {
                PlayRandomTwinkle();
                //twinkleAudio.pitch = Mathf.Lerp(0.8f, 1.5f, rb.linearVelocity.magnitude / 5f);
            }
        }
        else
        {
            if (streakParticles.isPlaying)
                streakParticles.Stop();

            twinkleAudio.Stop();
        }
    }

    public void Kick(Transform kicker)
    {
        if (kicked) return;
        kicked = true;
        GameManager.instance.UpdateScore(5);
        // apply knockback force
        if (rb != null && kicker != null)
        {
            Vector3 forceDir = (transform.position - kicker.position).normalized + Vector3.up * 0.3f;
            rb.AddForce(forceDir, ForceMode.Impulse);
            rb.AddTorque(Vector3.forward * 1f, ForceMode.Impulse);
        }
        StartCoroutine(Pop());
    }
    void PlayRandomTwinkle()
    {
        if (twinkleClips.Length == 0) return;

        int index = Random.Range(0, twinkleClips.Length);
        twinkleAudio.clip = twinkleClips[index];

        // Optional spice:
        twinkleAudio.pitch = Random.Range(0.9f, 1.2f);

        twinkleAudio.Play();
    }

    IEnumerator Pop()
    {
        yield return new WaitForSeconds(1.5f);
        popParticles.Play();
        yield return new WaitForSeconds(0.2f);
        int starBacon = Random.Range(1, 6);

        Damage.instance.BaconDamage(starBacon, transform.position, null, this);


        twinkleAudio.clip = popClip;
        twinkleAudio.volume = 0.8f;
        twinkleAudio.Play();

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

}