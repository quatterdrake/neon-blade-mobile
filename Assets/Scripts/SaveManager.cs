using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalKills;
    public int deaths;
    public int roomsCleared;

    // settings (week 12): slider values 0.0001..1, turned into dB by VolumeSettings
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;
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
        var data = new SaveData();   // defaults first: an old save without volume fields keeps them
        if (File.Exists(FilePath))
        {
            try { JsonUtility.FromJsonOverwrite(File.ReadAllText(FilePath), data); }
            catch (System.Exception e) { Debug.LogWarning("Save file broken, starting fresh: " + e.Message); }
        }
        return data;
    }

    // Reset progress clears stats but keeps the player's volume settings
    public static void Reset()
    {
        var old = Data;
        Data = new SaveData
        {
            masterVolume = old.masterVolume,
            musicVolume = old.musicVolume,
            sfxVolume = old.sfxVolume
        };
        Save();
    }
}
