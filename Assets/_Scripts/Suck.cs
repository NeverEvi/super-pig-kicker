using System.Collections.Generic;
using UnityEngine;

public class Suck : MonoBehaviour
{
    public Transform suckerPoint;

    public float suckRadius = 8f;
    public float pullStrength = 0.75f;
    public float maxPullSpeed = 2.5f;

    [Header("Global Weak Pull")]
    public float globalPullStrength = 0.03f;
    public float globalMaxPullSpeed = 0.5f;

    public LayerMask baconLayer;

    public Vector3 suckedSize = new(0.1f, 0.1f, 0.1f);

    private Collider[] hits = new Collider[64];
    private Dictionary<Rigidbody, Vector3> originalScales = new();

    void FixedUpdate()
    {
        CleanupDestroyedReferences();
        ApplyStrongRadiusPull();
    }


    void ApplyStrongRadiusPull()
    {
        int count = Physics.OverlapSphereNonAlloc(
            suckerPoint.position,
            suckRadius,
            hits,
            baconLayer
        );

        for (int i = 0; i < count; i++)
        {
            
            
            Collider hit = hits[i];
            if (hit == null) continue;

            if (!hit.TryGetComponent<AntiSuckTimer>(out AntiSuckTimer timer)) continue;
            if (timer.age < 10f) continue;

            Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
            if (rb == null) continue;

            if (!originalScales.ContainsKey(rb))
                originalScales[rb] = rb.transform.localScale;

            Vector3 toPoint = suckerPoint.position - rb.position;
            float distance = toPoint.magnitude;

            if (distance < 0.1f) continue;

            /*Vector3 dir = toPoint.normalized;

            float distancePercent = 1f - Mathf.Clamp01(distance / suckRadius);
            float strength = pullStrength * Mathf.Lerp(0.25f, 1f, distancePercent);

            rb.AddForce(dir * strength, ForceMode.Acceleration);*/
            Vector3 dir = toPoint.normalized;

            float distancePercent = 1f - Mathf.Clamp01(distance / suckRadius);
            float strength = pullStrength * Mathf.Lerp(0.25f, 1f, distancePercent);

            // Normal pull toward suckerPoint
            Vector3 forceDir = dir;

            // If bacon is behind the sucker, add lift + sideways swirl
            Vector3 toBacon = (rb.position - transform.position).normalized;
            float behindAmount = Vector3.Dot(transform.up, toBacon);

            // behindAmount < 0 means bacon is behind this object
            if (behindAmount > 0.15f && distance < suckRadius * 0.75f)
            {
                Vector3 sideDir = Vector3.Cross(transform.up, dir).normalized;

                // Pick direction based on which side it's already on
                float sideSign = Mathf.Sign(Vector3.Dot(transform.right, toBacon));
                sideDir *= sideSign;

                forceDir = (dir + Vector3.up * 1.25f + sideDir * 0.75f).normalized;
            }

            rb.AddForce(forceDir * strength, ForceMode.Acceleration);

            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxPullSpeed);
            
                float shrinkStart = 0.6f;
                float scaleT = Mathf.InverseLerp(shrinkStart, 1f, distancePercent);
                scaleT = Mathf.Pow(scaleT, 2.5f);

                Vector3 original = originalScales[rb];
                rb.transform.localScale = Vector3.Lerp(original, suckedSize, scaleT);
        }
    }

    void CleanupDestroyedReferences()
    {
        List<Rigidbody> deadKeys = new();

        foreach (var pair in originalScales)
        {
            if (pair.Key == null)
                deadKeys.Add(pair.Key);
        }

        foreach (Rigidbody rb in deadKeys)
            originalScales.Remove(rb);
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    void OnDrawGizmosSelected()
    {
        if (suckerPoint == null) return;
        Gizmos.DrawWireSphere(suckerPoint.position, suckRadius);
    }
}