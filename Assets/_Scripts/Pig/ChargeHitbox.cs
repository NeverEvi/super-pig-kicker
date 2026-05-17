using UnityEngine;

public class ChargeHitbox : MonoBehaviour
{
    public Pig pig; // reference to parent pig
    private bool active = false;

    public void Activate() => active = true;
    public void Deactivate() => active = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("BODY"))
        {
            int currentBacon = GameManager.instance.baconCount;

            int loss = pig.pigType == PigType.Devil
                ? Mathf.Max(150, currentBacon / 10)
                : Mathf.Max(30, currentBacon / 30);

            pig.combat.DamagePlayer(other.transform.position, loss);
            PlayerController playerBody = other.GetComponentInParent<PlayerController>();
            if (playerBody != null)
            {
                Vector3 knockDir = pig.transform.forward;
                playerBody.ApplyKnockback(knockDir, 6f, 10f);
            }
            active = false; // prevents multiple hits
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Fence"))
        {
            pig.combat.StopCharge();
        }
    }
}