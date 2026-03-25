using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Saved game to: " + SavePath);
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Loaded game from: " + SavePath);
        return data;
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}