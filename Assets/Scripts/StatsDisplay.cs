using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    public TMP_Text text;

    void Start()
    {
        var d = SaveManager.Data;
        text.text = "Kills: " + d.totalKills + "   Deaths: " + d.deaths + "   Rooms cleared: " + d.roomsCleared;
        Debug.Log("Save file: " + Application.persistentDataPath);
    }
}