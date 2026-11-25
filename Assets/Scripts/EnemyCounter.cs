using UnityEngine;
using TMPro;

public class EnemyCounter : MonoBehaviour
{
    public TMP_Text enemiesText;
    int remaining;

    void Start()
    {
        remaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
        UpdateText();
    }

    public void EnemyKilled()
    {
        remaining--;
        SaveManager.Data.totalKills++;
        if (remaining <= 0)
        {
            SaveManager.Data.roomsCleared++;
            AudioManager.PlayDelayed("win", 0.25f);   // after the hit sound, not on top of it
        }

        Debug.Log("EnemyKilled: totalKills now " + SaveManager.Data.totalKills);
        SaveManager.Save();

        if (enemiesText == null)
        {
            Debug.LogError("EnemyCounter: Enemies Text is not assigned on GameManager!");
            return;
        }

        enemiesText.text = remaining <= 0 ? "Room clear!" : "Enemies: " + remaining;
    }

    void UpdateText()
    {
        if (enemiesText != null)
            enemiesText.text = "Enemies: " + remaining;
    }
}