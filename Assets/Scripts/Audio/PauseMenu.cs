using UnityEngine;
using UnityEngine.SceneManagement;

// Pause = stop game time + switch the mixer to the Paused snapshot + show the panel.
public static class PauseMenu
{
    public static bool IsPaused { get; private set; }

    public static void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (AudioManager.Instance != null) AudioManager.Instance.SetPaused(true);

        SettingsPanel.Show("PAUSED",
            new SettingsPanel.Btn("Resume", Resume),
            new SettingsPanel.Btn("Main menu", ToMenu));
    }

    public static void Resume()
    {
        SettingsPanel.Hide();
        ResetState();
        if (AudioManager.Instance != null) AudioManager.Instance.SetPaused(false);
    }

    static void ToMenu()
    {
        SettingsPanel.Hide();
        ResetState();
        SceneManager.LoadScene("MainMenu");
    }

    // called on every scene load: a new scene never starts frozen
    public static void ResetState()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }
}
