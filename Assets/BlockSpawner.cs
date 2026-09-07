using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BlockSpawner : MonoBehaviour
{
    public GameObject[] tetrominos; // Blok prefab'leri
    public GameObject gameOverPanel; // Paneli buraya sürükleyeceğiz!

    void Start()
    {
        // Oyun başında panel kapalı olduğundan emin oluyoruz
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        NewRandomPiece();
    }

    public void NewRandomPiece()
    {
        int randomIndex = Random.Range(0, tetrominos.Length);
        Instantiate(tetrominos[randomIndex], transform.position, Quaternion.identity);
    }

    public void TriggerGameOver(int currentScore)
    {
        // Oyun bittiğinde sağ üstteki pause tuşunu ve anlık score text'ini gizleyelim
        GameObject pauseButton = GameObject.Find("PauseButton");
        if (pauseButton != null) pauseButton.SetActive(false);

        GameObject scoreText = GameObject.Find("ScoreText");
        if (scoreText != null) scoreText.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Skorları paneldeki textlere yazdıralım
            TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            int highScore = PlayerPrefs.GetInt("HighScore", 0);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
            }

            foreach (var t in texts)
            {
                if (t.gameObject.name == "FinalScoreText")
                {
                    t.text = "Score: " + currentScore;
                }
                else if (t.gameObject.name == "HighScoreText")
                {
                    t.text = "High Score: " + highScore;
                }
            }
        }
    }

    public void RestartGame()
    {
        TetrisPiece.ResetGrid();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}