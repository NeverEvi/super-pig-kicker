using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager instance;

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        string savedCode = PlayerPrefs.GetString("language", "");

        if (string.IsNullOrEmpty(savedCode))
            yield break;

        SetLanguage(savedCode, false);
    }

    public void SetLanguage(string code, bool save = true)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code == code);

        if (locale == null)
        {
            Debug.LogWarning($"Locale not found: {code}");
            return;
        }

        LocalizationSettings.SelectedLocale = locale;

        if (save)
        {
            PlayerPrefs.SetString("language", code);
            PlayerPrefs.Save();
        }

        ShopManager.instance?.UpdateUI();
    }
}