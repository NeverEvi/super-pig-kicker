using System.Collections;
using UnityEngine;

public class Summoning : MonoBehaviour
{
    public static Summoning instance;

    public GameObject Pentagram;
    public GameObject Fire1;
    public GameObject Fire2;
    public GameObject Fire3;
    public GameObject Fire4;
    public GameObject Fire5;
    public GameObject FireMain;
    public GameObject DEVIL;
    public GameObject DEVILHead;
    public GameObject player;

    [Header("Settings")]
    public float pentagramTargetScale = 1.45f;
    public float pentagramScaleTime = 1f;
    public float fireDelay = 0.3f;
    public float mainFireDelay = 0.5f;
    public float devilRiseDelay = 0.2f;
    public float devilRiseTime = 2f;
    public float devilStartY = -3f;
    public float devilEndY = 1f;

    public bool isSummoning = false;
    private PlayerController pc;
    public GameObject EndCredits;
    public RectTransform CreditsTransform;

    void Awake() => instance = this;
    private void Start()
    {
        pc = PlayerController.instance;
    }
    public void SUMMON()
    {
        if (isSummoning) return;
        StartCoroutine(SummonRoutine());
    }

    private IEnumerator SummonRoutine()
    {
        isSummoning = true;

        pc.canMove = false;
        Cursor.lockState = CursorLockMode.None;
        LoopingMusic.instance.FadeToSong(3);
        //push (lerp) the player  over 1 second back to the Mainmenu.instance.spawnPoint.transform.position
        //lerp to Rotate the player on the y-Axis only to face the devil pig, while lerping the x-rotation of the main camera to zero
        Vector3 playerStartPos = player.transform.position;
        Vector3 playerEndPos = MainMenu.instance.spawnPoint.transform.position;

        Quaternion playerStartRot = player.transform.rotation;
        Vector3 lookDir = DEVIL.transform.position - playerEndPos;
        lookDir.y = 0f;
        Quaternion playerEndRot = lookDir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(lookDir) : playerStartRot;

        Transform cam = Camera.main.transform;
        Vector3 camStartEuler = cam.localEulerAngles;
        Vector3 camEuler;
        float camStartX = camStartEuler.x;
        if (camStartX > 180f) camStartX -= 360f;

        float moveTimer = 0f;
        float moveDuration = 1f;

        while (moveTimer < moveDuration)
        {
            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDuration;


            player.transform.SetPositionAndRotation(
                Vector3.Lerp(playerStartPos, playerEndPos, t),
                Quaternion.Slerp(playerStartRot, playerEndRot, t)
                );
            camEuler = cam.localEulerAngles;
            float newX = Mathf.Lerp(camStartX, 0f, t);
            cam.localEulerAngles = new Vector3(newX, camEuler.y, camEuler.z);

            yield return null;
        }

        player.transform.SetPositionAndRotation(playerEndPos, playerEndRot);
        
        camEuler = cam.localEulerAngles;
        cam.localEulerAngles = new Vector3(0f, camEuler.y, camEuler.z);
        
        Pentagram.SetActive(true);

        Vector3 pentagramStartScale = Vector3.zero;
        Vector3 pentagramEndScale = Vector3.one * pentagramTargetScale;
        Pentagram.transform.localScale = pentagramStartScale;

        yield return StartCoroutine(LerpScale(Pentagram.transform, pentagramStartScale, pentagramEndScale, pentagramScaleTime));

        //start very subtle screen shake
        ScreenShake.instance.Shake(20f, 0.16f);

        yield return new WaitForSeconds(0.3f);
        Fire1.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire2.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire3.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire4.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire5.SetActive(true);

        yield return new WaitForSeconds(mainFireDelay);
        FireMain.SetActive(true);

        yield return new WaitForSeconds(devilRiseDelay);

        DEVIL.SetActive(true);
        //DayCycle.instance.StopAllCoroutines();
        //DayCycle.instance.bossFight = true;
        Debug.Log($"Summoning using DayCycle instance: {DayCycle.instance.name} id={DayCycle.instance.GetInstanceID()}");
        DayCycle.instance.StartBossLighting();

        Vector3 devilPos = DEVIL.transform.position;
        devilPos.y = devilStartY;
        DEVIL.transform.position = devilPos;

        yield return StartCoroutine(LerpYPosition(DEVIL.transform, devilStartY, devilEndY, devilRiseTime));

        if (DEVIL.TryGetComponent<BoxCollider>(out BoxCollider box)) box.enabled = true;

        if (DEVIL.TryGetComponent<Pig>(out Pig pig)) pig.enabled = true;
        if(GameManager.instance.newGamePlus > 0)
        {
            int hpMod = (GameManager.instance.newGamePlus * 100);
            pig.hp += hpMod;
            pig.mhp += hpMod;
        }

        if (DEVIL.TryGetComponent<PigCombat>(out PigCombat combat)) combat.enabled = true;

        if (DEVIL.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.useGravity = true;

        yield return new WaitForSeconds(0.2f);

        Fire1.SetActive(false);
        Fire2.SetActive(false);
        Fire3.SetActive(false);
        Fire4.SetActive(false);
        Fire5.SetActive(false);
        FireMain.SetActive(false);

        //stop screen shake
        ScreenShake.instance.Shake(0.1f, 0.1f);

        pc.canMove = true; 
        Cursor.lockState = CursorLockMode.Locked;
        isSummoning = false;
    }

    private IEnumerator LerpScale(Transform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale;
    }

    //make player camera follow the devil starting halfway through LerpYPosition
    private IEnumerator LerpYPosition(Transform target, float startY, float endY, float duration)
    {
        float timer = 0f;
        Vector3 pos;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            pos = target.position;
            pos.y = Mathf.Lerp(startY, endY, t);
            target.position = pos;

            yield return null;
        }

        pos = target.position;
        pos.y = endY;
        target.position = pos;
    }
    private IEnumerator LerpYPositionAndSpin(Transform target, float startY, float endY, float duration)
    {
        float timer = 0f;
        Vector3 pos = target.position;

        float spinSpeed = 200f;
        ScreenShake.instance.Shake(duration, 0.1f);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            pos = target.position;
            pos.y = Mathf.Lerp(startY, endY, t);
            target.position = pos;

            target.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

            yield return null;
        }

        pos = target.position;
        pos.y = endY;
        target.position = pos;
    }


    public void Endgame()
    {
        StartCoroutine(EndGameRoutine());
    }

    private IEnumerator EndGameRoutine()
    {
        isSummoning = true;
        pc.canMove = false;
        Cursor.lockState = CursorLockMode.None;

        LoopingMusic.instance.FadeToSong(1);

        if (DEVIL.TryGetComponent<BoxCollider>(out BoxCollider box)) box.enabled = false;

        if (DEVIL.TryGetComponent<Pig>(out Pig pig)) 
        {
            pig.hp = pig.mhp;
            pig.beenKicked = false;
            pig.enabled = false;
        }

        if (DEVIL.TryGetComponent<PigCombat>(out PigCombat combat))
        {
            combat.StopAllCoroutines();
            combat.enabled = false; 
        }

        if (DEVIL.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.useGravity = false;


        Vector3 playerStartPos = player.transform.position;
        Vector3 playerEndPos = MainMenu.instance.spawnPoint.transform.position;
        Vector3 devilStartPos = DEVIL.transform.position;
        Vector3 devilEndPos = new Vector3(-0.94f, 1f, 3.59f);//MainMenu.instance.devilStartPoint.transform.position;

        Quaternion playerStartRot = player.transform.rotation;
        Vector3 lookDir = devilEndPos - playerEndPos;

        lookDir.y = 0f;
        Quaternion playerEndRot = lookDir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(lookDir) : playerStartRot;

        Transform cam = Camera.main.transform;
        Vector3 camStartEuler = cam.localEulerAngles;
        float camStartX = camStartEuler.x;
        if (camStartX > 180f) camStartX -= 360f;

        float moveTimer = 0f;
        float moveDuration = 1f;
        Vector3 camEuler;

        while (moveTimer < moveDuration)
        {
            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDuration;

            player.transform.SetPositionAndRotation(
                Vector3.Lerp(playerStartPos, playerEndPos, t),
                Quaternion.Slerp(playerStartRot, playerEndRot, t)
                );
            DEVIL.transform.position = Vector3.Lerp(devilStartPos, devilEndPos, t);

            camEuler = cam.localEulerAngles;
            float newX = Mathf.Lerp(camStartX, 0f, t);
            cam.localEulerAngles = new Vector3(newX, camEuler.y, camEuler.z);

            yield return null;
        }

        player.transform.SetPositionAndRotation(playerEndPos, playerEndRot);
        
        DEVIL.transform.position = devilEndPos;
        camEuler = cam.localEulerAngles;
        cam.localEulerAngles = new Vector3(0f, camEuler.y, camEuler.z);
        


        
        //start very subtle screen shake

        yield return new WaitForSeconds(0.3f);
        Fire5.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire4.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire3.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire2.SetActive(true);

        yield return new WaitForSeconds(fireDelay);
        Fire1.SetActive(true);

        yield return new WaitForSeconds(mainFireDelay);
        FireMain.SetActive(true);

        yield return new WaitForSeconds(devilRiseDelay);

        Debug.Log($"Summoning using DayCycle instance: {DayCycle.instance.name} id={DayCycle.instance.GetInstanceID()}");
        DayCycle.instance.EndBossLighting();

        Vector3 devilPos = DEVIL.transform.position;
        devilPos.y = devilEndY;
        DEVIL.transform.position = devilPos;

        yield return StartCoroutine(LerpYPositionAndSpin(DEVIL.transform, devilEndY, devilStartY, devilRiseTime));

        

        yield return new WaitForSeconds(0.2f);

        Fire1.SetActive(false);
        Fire2.SetActive(false);
        Fire3.SetActive(false);
        Fire4.SetActive(false);
        Fire5.SetActive(false);
        FireMain.SetActive(false);

        Vector3 pentagramStartScale = Vector3.zero;
        Vector3 pentagramEndScale = Vector3.one * pentagramTargetScale;
        Pentagram.transform.localScale = pentagramEndScale;

        yield return StartCoroutine(LerpScale(Pentagram.transform, pentagramEndScale, pentagramStartScale, pentagramScaleTime));

        //stop screen shake
        pc.canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        DEVIL.SetActive(false);
        isSummoning = false;
        GameManager.instance.UpdateScore(100);
        yield return StartCoroutine(MainMenu.instance.FadeBlackout(0f, 1f));
        CreditsManager.instance.OpenCredits(true);
    }
    
}