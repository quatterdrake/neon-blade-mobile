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
        if (remaining <= 0) enemiesText.text = "Room clear!";
        else UpdateText();
    }

    void UpdateText()
    {
        enemiesText.text = "Enemies: " + remaining;
    }
}