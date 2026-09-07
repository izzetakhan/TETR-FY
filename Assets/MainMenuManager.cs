using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("tetris");
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapatıldı!");
        Application.Quit();
    }
}