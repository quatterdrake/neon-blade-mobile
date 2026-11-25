using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadMenu()
    {
        PauseMenu.ResetState();
        SceneManager.LoadScene("MainMenu");
    }

    // Settings button in the main menu
    public void OpenSettings()
    {
        SettingsPanel.Show("SETTINGS", new SettingsPanel.Btn("Back", () => SettingsPanel.Hide()));
    }

    // Pause button in the game scene
    public void PauseGame()
    {
        PauseMenu.Pause();
    }

    public void ResetProgress()
    {
        SaveManager.Reset();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}