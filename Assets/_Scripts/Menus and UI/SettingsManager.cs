using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [SerializeField] private PlayerController playerController;
    public Slider volumeSlider;
    public Slider volumeSlider2;
    
    public Slider sensitivitySlider;
    public Slider sensitivitySlider2;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        SetVolume(savedVolume);

        float savedMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        SetMouseSensitivity(savedMouseSensitivity);
    }

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider2.value = PlayerPrefs.GetFloat("MasterVolume", 1f);

        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        sensitivitySlider2.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float value)
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
            playerController.SetMouseSensitivity(value);

        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}