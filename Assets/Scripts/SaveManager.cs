using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalKills;
    public int deaths;
    public int roomsCleared;
}

public static class SaveManager
{
    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData Data = Load();

    public static void Save()
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
    }

    static SaveData Load()
    {
        if (File.Exists(FilePath))
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
        return new SaveData();
    }

    public static void Reset()
    {
        Data = new SaveData();
        Save();
    }
}