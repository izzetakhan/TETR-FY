using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private float moveSpeed = 60f; // UI için uygun süzülme hızı
    private float alphaSpeed = 1.5f;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        if (textMesh != null)
            textColor = textMesh.color;
    }

    void Update()
    {
        // UI elemanını yukarı süzdür
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += new Vector2(0, moveSpeed * Time.deltaTime);
        }

        // Şeffaflaşarak kaybolma
        if (textMesh != null)
        {
            textColor.a -= alphaSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(string message)
    {
        if (textMesh != null)
        {
            textMesh.text = message;
        }
    }
}