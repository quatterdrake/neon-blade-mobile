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
        SceneManager.LoadScene("MainMenu");
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