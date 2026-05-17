using UnityEngine;

public class SuckerWander : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float turnSpeed = 4f;

    public float visualRotationOffsetY = -90f;

    private Vector3 moveDir;
    public LayerMask bounceLayers;

    void Start()
    {
        PickRandomDirection();
    }

    void Update()
    {
        transform.position += moveDir * (moveSpeed * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(moveDir, Vector3.up) *
                Quaternion.Euler(0f, visualRotationOffsetY, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, bounceLayers))
        {
            PickRandomDirection();
        }
    }

    
    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }
}