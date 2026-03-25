using System.Collections;
using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    public float kickRange = 1f;
    public float kickTime = 0.3f;

    private bool isKicking = false;
    private Vector3 startPos;
    public int kickStrength = 1;

    private NewControls inputActions;
    void Awake()
    {
        inputActions = new NewControls();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Kick.performed += ctx => StartKick();
    }

    void OnDisable()
    {
        inputActions.Player.Kick.performed -= ctx => StartKick();
        inputActions.Player.Disable();
    }

    void Start()
    {
        startPos = transform.localPosition;
    }

    void StartKick()
    {
        if (!isKicking)
            StartCoroutine(KickCoroutine());
    }

    IEnumerator KickCoroutine()
    {
        isKicking = true;

        Vector3 forwardPos = startPos + new Vector3(0, 0, kickRange);
        Vector3 returnPos = new (startPos.x, startPos.y, 0.1f);

        float timer = 0f;

        // forward
        while (timer < kickTime)
        {
            transform.localPosition = Vector3.Lerp(startPos, forwardPos, timer / kickTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = forwardPos;

        timer = 0f;

        // back
        while (timer < kickTime)
        {
            transform.localPosition = Vector3.Lerp(forwardPos, returnPos, timer / kickTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = returnPos;

        isKicking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Pig pig = other.GetComponent<Pig>();
        if (pig != null && isKicking)
        {
            pig.Kick(kickStrength, transform);
            return;
        }
        Crate crate = other.GetComponent<Crate>();
        if(crate != null && isKicking)
        {
            crate.Kick();
        }    
    }
}
