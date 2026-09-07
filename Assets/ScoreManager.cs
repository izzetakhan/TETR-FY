using UnityEngine;
using TMPro; // Eğer TextMeshPro kullanıyorsan bu kalsın. (Normal UI Text ise using UnityEngine.UI; yapmalısın)

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;

    [Header("UI Referansı")]
    public TMP_Text scoreText; // Sahnedeki skor yazısını buraya bağlayacağız

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        score = 0;          // Oyun başlar başlamaz skoru kesin olarak 0 yap
        UpdateScoreUI();    // Ekrandaki skoru güncelle
    }

    public void AddScore(int amount, Vector3 position)
    {
        score += amount;
        Debug.Log("Güncel Skor: " + score);

        UpdateScoreUI(); // Skor her arttığında ekrandaki yazıyı güncelle
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogWarning("Score Text referansı atanmamış!");
        }
    }
}