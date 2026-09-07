using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    public void TogglePause() // public kelimesi burada şart
    {
        bool isPaused = pausePanel.activeSelf;
        pausePanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1f : 0f;
    }

    public void ContinueGame() // public
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame() // public
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuickGame() // public
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}