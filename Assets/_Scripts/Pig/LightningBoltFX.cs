using UnityEngine;
using System.Collections;

public class LightningBoltFX : MonoBehaviour
{
    public float life = 0.2f;
    public float flickerInterval = 0.025f;
    public bool randomZRotation = true;
    public GameObject shockParticle;
    private Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        if (randomZRotation)
        {
            transform.Rotate(0f, Random.Range(0f, 360f), 0f);
        }
        Instantiate(shockParticle, transform.position, Quaternion.identity);
        StartCoroutine(FlickerThenDie());
    }

    IEnumerator FlickerThenDie()
    {
        float timer = 0f;
        bool visible = true;

        while (timer < life)
        {
            visible = !visible;

            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = visible;
            }

            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        Destroy(gameObject);
    }
}
