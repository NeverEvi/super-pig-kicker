using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private readonly List<string> localeCodes = new()
    {
        "en",
        "ja",
        "de",
        "es",
        "uk",
        "fr",
        "zh-Hans",
        "ru"
    };

    private IEnumerator Start()
    {
        //PlayerPrefs.DeleteKey("language");
        //PlayerPrefs.Save();

        yield return LocalizationSettings.InitializationOperation;

        dropdown.ClearOptions();

        dropdown.AddOptions(new List<string>
        {
            "English",
            "日本語",
            "Deutsch",
            "Español",
            "Українська",
            "Français",
            "简体中文",
            "Русский"
        });

        string savedCode = PlayerPrefs.GetString("language", "");

        int index = 0;

        if (!string.IsNullOrEmpty(savedCode))
        {

            Debug.Log("Saved language: " + savedCode);
            index = localeCodes.IndexOf(savedCode);
            if (index < 0) index = 0;
            var savedLocale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code == savedCode);

            if (savedLocale != null)
                LocalizationSettings.SelectedLocale = savedLocale;
            ShopManager.instance?.UpdateUI();
        }
        else
        {
            string currentCode = LocalizationSettings.SelectedLocale.Identifier.Code;
            Debug.Log("Detected language: " + currentCode);
            index = localeCodes.IndexOf(currentCode);
            if (index < 0) index = 0;
        }

        dropdown.SetValueWithoutNotify(index);
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(SetLanguage);
    }

    public void SetLanguage(int index)
    {
        if (index < 0 || index >= localeCodes.Count) return;

        string code = localeCodes[index];

        LocalizationManager.instance.SetLanguage(code);
    }
}