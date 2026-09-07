using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float vanishTimer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(int scoreAmount)
    {
        textMesh.text = "+" + scoreAmount.ToString();
        vanishTimer = 1.0f; // 1 saniye ekranda kalacak
    }

    void Update()
    {
        // Yukarı doğru süzülme efekti
        transform.position += new Vector3(0, 5f * Time.deltaTime, 0);

        vanishTimer -= Time.deltaTime;
        if (vanishTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}