using System.Collections;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public static Damage instance;
    int currentBacon;
    public GameObject baconPrefab;

    private void Awake() => instance = this;
    
    public void BaconDamage(int damage, Vector3 hitPosition, Pig pig=null, Star star=null)
    {
        int loss; //for player bacon loss on damage
        int spill; //amount to spill

        if (!GameManager.instance.immune) 
            StartCoroutine(Immunity((pig != null || star != null) ? 0.2f : 0.4f));

        if (pig != null)
        {
            spill = pig.baconPerKick* damage;
        }
        else if (star != null)
        {
            spill = damage;
        }
        else 
        {
            currentBacon = GameManager.instance.baconCount;

            loss = Mathf.Min(damage, currentBacon); //loss is the damage or the total, whatever is lower (to avoid negative)
            if (loss <= 0) return;
            spill = loss / 2; //spill is half the loss
            GameManager.instance.SpendBacon(loss); //so get rid of the whole amount

        }
        while (spill > 0)
        {
            ////// MAX STACK DEFINITION //////
            int baseStack = 10 * PlayerKick.instance.kickStrength;
            int maxStack = baseStack;
            if(star == null)
            {
                if (ShopManager.instance.alienBought) maxStack += 10;
                maxStack += (GameManager.instance.angelKills * 2);
            
                float scale = Mathf.Clamp(spill / 200000f, 1f, 10f);
                maxStack = Mathf.RoundToInt(maxStack * scale);
            }
            else maxStack = 1;

            int maxSpawns = 200;
            int estimatedSpawns = Mathf.CeilToInt((float)spill / maxStack);
            if (estimatedSpawns > maxSpawns)
            {
                maxStack = Mathf.CeilToInt((float)spill / maxSpawns);
            }
            //////////////////////////////////

            int stackAmount = Mathf.Min(maxStack, spill);

            ////// INSTANTIATE BACON //////
            GameObject newBacon = Instantiate(baconPrefab, pig != null ? pig.spawnPoint.position: hitPosition, Quaternion.identity);
            Bacon bacon = newBacon.GetComponentInChildren<Bacon>();
            if (bacon != null) bacon.baconValue = stackAmount;

            //////////// Bacon Color Adjustments
            Renderer baconSkin = newBacon.GetComponentInChildren<Renderer>();
            float roll = Random.Range(1, 100);
            if (pig != null) 
            {
                switch (pig.pigType)
                {
                    case PigType.Pig:
                        if (roll <= 2)
                        {
                            bacon.baconValue += 1;
                        }
                        break;
                    case PigType.Golden:
                        if (roll <= 20)
                        {
                            bacon.baconValue += Random.Range(1, 6);
                            if(roll <= 10)
                            {
                                bacon.baconValue += Random.Range(1, 3);
                            }
                            if(roll == 1)
                            {
                                bacon.baconValue += 4;
                            }
                            baconSkin.material.color = Color.gold;
                        }
                        break;
                    case PigType.Alien:
                        baconSkin.material.color = Color.darkGreen;
                        if (roll <= 10)
                        {
                            bacon.baconValue += Random.Range(1, 5);
                        }
                        break;
                    case PigType.Angel:
                        baconSkin.material.color = Color.white;
                        break;
                }
            }
            ///////////////////////////////

            ////// SEND BACON FLYING //////
            if (newBacon.TryGetComponent<Rigidbody>(out Rigidbody brb) && Center.instance != null)
            {
                Vector3 toCenter = (Center.instance.transform.position - (pig != null? pig.spawnPoint.position : hitPosition));
                toCenter.y = 0f;
                toCenter = toCenter.sqrMagnitude > 0.001f ? toCenter.normalized : Vector3.zero;

                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;

                // Mostly random, slightly pulled toward center
                Vector3 launchDir = (randomDir + toCenter * 0.6f).normalized;

                // Add upward pop
                launchDir += Vector3.up * 0.2f;
                launchDir.Normalize();

                float launchForce = Random.Range(0.5f, 0.9f);

                brb.AddForce(launchDir * launchForce, ForceMode.Impulse);
            }
            ///////////////////////////////
            spill -= stackAmount;
        }
    }


    IEnumerator Immunity(float timer)
    {
        float time = 0;
        GameManager.instance.immune = true;
        while (time < timer)
        {
            time += Time.deltaTime;
            yield return null;
        }
        GameManager.instance.immune = false;
    }
}
