using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DayCycle : MonoBehaviour
{

    public static DayCycle instance;
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxDay;
    public Light MainLight;
    public float dayDuration = 45f;
    public float nightDuration = 30f;

    private float timer = 0f;
    public bool isDay = true;

    [Header("Light Settings")]
    public Vector3 dayRotation = new(50f, -30f, 0f);
    public Vector3 nightRotation = new(25f, -50f, 0f);

    public float dayIntensity = 1.1f;
    public float nightIntensity = 0.5f;

    [Header("Boss Fight")]
    public bool bossFight = false;
    private bool bossColorLerped = false;
    public Color bossAmbientColor = new(0.7f, 0f, 0f); // #B40000
    public Color normalAmbientColor = Color.white;

    public Color bossSkyTint = new(0.4f, 0f, 0f, 0.5f); // #67000080
    public Color normalSkyTint = new(0.5f, 0.5f, 0.5f, 0.5f); // #80808080

    private Material skyboxMaterial;

    void Awake() => instance = this; 
    private void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
        skyboxMaterial.SetTexture("_Texture1", skyboxDay);
        skyboxMaterial.SetTexture("_Texture2", skyboxNight);
        skyboxMaterial.SetFloat("_Blend", 0);
    }
    void Update()
    {
        if (!bossFight)
        {
            if (bossColorLerped) 
            {
                bossColorLerped = false;
                EndBossLighting();
            }
            timer += Time.deltaTime;
            float t = isDay ? timer / dayDuration : timer / nightDuration;
            if (t >= 1f)
            {
                timer = 0f;
                isDay = !isDay;
                if (isDay)
                {
                    GameManager.instance.UpdateScore(10);
                    StartCoroutine(LerpSkybox(skyboxNight, skyboxDay, 2f));
                    StartCoroutine(LerpLight(nightRotation, dayRotation, nightIntensity, dayIntensity, 2f));
                }
                else
                {
                    StartCoroutine(LerpSkybox(skyboxDay, skyboxNight, 2f));
                    StartCoroutine(LerpLight(dayRotation, nightRotation, dayIntensity, nightIntensity, 2f));
                }
            }
        }
        else if (!bossColorLerped) 
        { 
            bossColorLerped = true;
            StartBossLighting();
        }
        

    }
    public void Flash()  => StartCoroutine(FlashRoutine());
    public void Darken() => StartCoroutine(DarkenRoutine());

    public void StartBossLighting()
    {
        StopAllCoroutines();

        skyboxMaterial = RenderSettings.skybox;

        bossFight =true; bossColorLerped=true;

        StartCoroutine(StartBossLightingRoutine());
    }

    private IEnumerator StartBossLightingRoutine()
    {
        StartCoroutine(LerpAmbient(RenderSettings.ambientLight, bossAmbientColor, 2f));

        if (!isDay) yield return StartCoroutine(LerpSkybox(skyboxNight, skyboxDay, 2f));
        
        yield return StartCoroutine(LerpSkyTint(GetSkyTint(), bossSkyTint, 2f));
    }

    public void EndBossLighting()
    {
        bossFight = false;
        StartCoroutine(LerpAmbient(RenderSettings.ambientLight, normalAmbientColor, 2f));
        StartCoroutine(LerpSkyTint(GetSkyTint(), normalSkyTint, 2f));
    }
    private Color GetSkyTint()
    {
        if (skyboxMaterial.HasProperty("_Tint1")) return skyboxMaterial.GetColor("_Tint1");

        return Color.white;
    }
    private IEnumerator LerpAmbient(Color start, Color end, float time)
    {
        //Debug.Log("Lerping Ambient color from "+start+" to "+ end);
        
        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;
            RenderSettings.ambientLight = Color.Lerp(start, end, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        RenderSettings.ambientLight = end;
    }

    private IEnumerator LerpSkyTint(Color start, Color end, float time)
    {
        //Debug.Log("Lerping Sky tint from " + start + " to " + end);
        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;

            if (skyboxMaterial.HasProperty("_Tint1"))
            {
                Color tint = Color.Lerp(start, end, t);
                skyboxMaterial.SetColor("_Tint1", tint);
            }
            if (skyboxMaterial.HasProperty("_Tint2"))
            {
                Color tint2 = Color.Lerp(start, end, t);
                skyboxMaterial.SetColor("_Tint2", tint2);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (skyboxMaterial.HasProperty("_Tint1")) skyboxMaterial.SetColor("_Tint1", end);
    }
    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        //Debug.Log("Lerping Skybox from " + a + " to " + b);
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);

    }
    private IEnumerator LerpLight(Vector3 startRot, Vector3 endRot, float startIntensity, float endIntensity, float time)
    {
        float elapsed = 0f;

        Quaternion startQ = Quaternion.Euler(startRot);
        Quaternion endQ = Quaternion.Euler(endRot);

        while (elapsed < time)
        {
            float t = elapsed / time;

            // Smooth interpolation
            MainLight.transform.rotation = Quaternion.Slerp(startQ, endQ, t);
            MainLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap to final values
        MainLight.transform.rotation = endQ;
        MainLight.intensity = endIntensity;
    }

    private IEnumerator FlashRoutine()
    {
        float originalIntensity = isDay ? 1f : 0.5f;
        float peakIntensity = originalIntensity + 2f; // adjust how bright the flash is

        float lerpTime = 0.1f; // time to lerp up or down
        float holdTime = 0.3f; // time to hold at peak

        // Lerp up
        float elapsed = 0f;
        while (elapsed < lerpTime)
        {
            MainLight.intensity = Mathf.Lerp(originalIntensity, peakIntensity, elapsed / lerpTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        MainLight.intensity = peakIntensity;

        yield return new WaitForSeconds(holdTime);

        // Lerp back down
        elapsed = 0f;
        while (elapsed < lerpTime)
        {
            MainLight.intensity = Mathf.Lerp(peakIntensity, originalIntensity, elapsed / lerpTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        MainLight.intensity = originalIntensity;
    }
    private IEnumerator DarkenRoutine()
    {
        float originalIntensity = isDay ? 1f : 0.5f;
        float dimmedIntensity = originalIntensity * 0.2f; // adjust how dim it gets

        float lerpTime = 0.2f; // lerp duration each way
        float holdTime = 0.6f; // hold at dimmed intensity

        // Lerp down
        float elapsed = 0f;
        while (elapsed < lerpTime)
        {
            MainLight.intensity = Mathf.Lerp(originalIntensity, dimmedIntensity, elapsed / lerpTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        MainLight.intensity = dimmedIntensity;

        yield return new WaitForSeconds(holdTime); // total dimmed hold as requested

        // Lerp back up
        elapsed = 0f;
        while (elapsed < lerpTime)
        {
            MainLight.intensity = Mathf.Lerp(dimmedIntensity, originalIntensity, elapsed / lerpTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        MainLight.intensity = originalIntensity;
    }


    private void OnApplicationQuit()
    {
        skyboxMaterial.SetColor("_Tint1", normalSkyTint);
        skyboxMaterial.SetColor("_Tint2", normalSkyTint);
        Application.Quit();
    }
}