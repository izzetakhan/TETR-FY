using UnityEngine;

public class TetrisPiece : MonoBehaviour
{
    public float fallSpeed = 0.5f;
    private float previousTime;
    private float spawnTime;              // Bloğun doğduğu anı tutar
    private bool hasMovedDown = false; // Bloğun en az bir kez aşağı inip inmediğini kontrol eder

    public float minX = -14.28f;
    public float maxX = 12f;
    public float minY = -23f;

    public static int width = 10;
    public static int height = 60;
    private static Transform[,] grid = new Transform[width, height];

    private BoxCollider2D boxCol;

    private int currentGridX = 0;
    private int currentGridY = 0;

    // --- SÜZÜLME (LERP) İÇİN DEĞİŞKENLER ---
    private Vector3 targetPosition;
    public float smoothSpeed = 16f; // Süzülme akıcılık hızı

    public static TetrisPiece activeInstance;

    void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        activeInstance = this;
    }

    void Start()
    {
        previousTime = Time.time;
        spawnTime = Time.time; // Doğduğu saniyeyi kaydediyoruz
        boxCol = GetComponent<BoxCollider2D>();

        currentGridX = Mathf.RoundToInt((transform.position.x - minX) / 2.54f);
        currentGridY = Mathf.RoundToInt((transform.position.y - minY) / 2.54f);

        // Başlangıç pozisyonunu direkt hedef yapıp ışınlayalım ki ilk karede kaymasın
        targetPosition = new Vector3(minX + (currentGridX * 2.54f), minY + (currentGridY * 2.54f), transform.position.z);
        transform.position = targetPosition;

        // Doğduğu konumda halihazırda yerleşmiş bir blok var mı kontrol ediyoruz
        foreach (Transform child in transform)
        {
            int checkX = Mathf.RoundToInt((child.position.x - minX) / 2.54f);
            int checkY = Mathf.RoundToInt((child.position.y - minY) / 2.54f);

            if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
            {
                if (grid[checkX, checkY] != null)
                {
                    GameOver();
                    break;
                }
            }
        }
    }

    void GameOver()
    {
        Debug.Log("OYUN BİTTİ!");

        BlockSpawner spawner = FindObjectOfType<BlockSpawner>();
        if (spawner != null)
        {
            spawner.enabled = false;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.score : 0;
            spawner.TriggerGameOver(currentScore);
        }

        Destroy(gameObject);
    }

    public void RestartGame()
    {
        ResetGrid();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (activeInstance != this) return;

        // --- YUMUŞAK SÜZÜLME (LERP) ---
        // Her frame pozisyonu hedefe doğru akıcı bir şekilde yaklaştırıyoruz
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // --- 1. KLASİK KLAVYE KONTROLLERİ ---
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            RotatePiece();
        }

        // --- 2. MOBİL DOKUNMATİK / TAP KONTROLLERİ ---
        bool inputTriggered = false;
        Vector3 inputPosition = Vector3.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                inputTriggered = true;
                inputPosition = touch.position;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputTriggered = true;
            inputPosition = Input.mousePosition;
        }

        if (inputTriggered)
        {
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            bool clickedOnThisPiece = false;
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
                {
                    clickedOnThisPiece = true;
                }
            }

            if (clickedOnThisPiece)
            {
                RotatePiece();
            }
            else
            {
                if (inputPosition.y < Screen.height * 0.25f)
                {
                    DropPiece();
                }
                else
                {
                    if (inputPosition.x < Screen.width / 2f)
                    {
                        MoveLeft();
                    }
                    else
                    {
                        MoveRight();
                    }
                }
            }
        }

        // --- 3. DÜŞME MANTIĞI ---
        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ? fallSpeed / 10 : fallSpeed))
        {
            if (CanMoveTo(currentGridX, currentGridY - 1))
            {
                currentGridY--;
                UpdateWorldPosition();
                hasMovedDown = true;
            }
            else
            {
                if (hasMovedDown || (Time.time - spawnTime > 0.5f))
                {
                    LandPiece();
                    return;
                }
            }

            previousTime = Time.time;
        }
    }

    public void MoveRight()
    {
        if (CanMoveTo(currentGridX + 1, currentGridY))
        {
            currentGridX++;
            UpdateWorldPosition();
        }
    }

    public void MoveLeft()
    {
        if (CanMoveTo(currentGridX - 1, currentGridY))
        {
            currentGridX--;
            UpdateWorldPosition();
        }
    }

    public void RotatePiece()
    {
        transform.Rotate(0, 0, 90);

        Vector3 angles = transform.eulerAngles;
        angles.z = Mathf.Round(angles.z / 90f) * 90f;
        transform.eulerAngles = angles;

        if (!IsValidPosition())
        {
            transform.Rotate(0, 0, -90);
            angles = transform.eulerAngles;
            angles.z = Mathf.Round(angles.z / 90f) * 90f;
            transform.eulerAngles = angles;
        }
    }

    public void DropPiece()
    {
        if (CanMoveTo(currentGridX, currentGridY - 1))
        {
            currentGridY--;
            UpdateWorldPosition();
            hasMovedDown = true;
        }
        else
        {
            if (hasMovedDown || (Time.time - spawnTime > 0.5f))
            {
                LandPiece();
            }
        }
    }

    void UpdateWorldPosition()
    {
        // Pozisyonu direkt ışınlamak yerine hedef pozisyonu güncelliyoruz, Lerp süzülmesini sağlayacak
        targetPosition = new Vector3(minX + (currentGridX * 2.54f), minY + (currentGridY * 2.54f), transform.position.z);
    }

    bool CanMoveTo(int targetX, int targetY)
    {
        int deltaX = targetX - currentGridX;
        int deltaY = targetY - currentGridY;

        foreach (Transform child in transform)
        {
            Vector3 targetChildPos = child.position + new Vector3(deltaX * 2.54f, deltaY * 2.54f, 0);
            int checkX = Mathf.RoundToInt((targetChildPos.x - minX) / 2.54f);
            int checkY = Mathf.RoundToInt((targetChildPos.y - minY) / 2.54f);

            if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
                return false;

            if (grid[checkX, checkY] != null)
                return false;
        }
        return true;
    }

    bool IsValidPosition()
    {
        foreach (Transform child in transform)
        {
            int checkX = Mathf.RoundToInt((child.position.x - minX) / 2.54f);
            int checkY = Mathf.RoundToInt((child.position.y - minY) / 2.54f);

            if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
                return false;

            if (grid[checkX, checkY] != null)
                return false;
        }
        return true;
    }

    void LandPiece()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }

        int childCount = transform.childCount;
        Transform[] childrenArray = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            childrenArray[i] = transform.GetChild(i);
        }

        foreach (Transform child in childrenArray)
        {
            int gridX = Mathf.RoundToInt((child.position.x - minX) / 2.54f);
            int gridY = Mathf.RoundToInt((child.position.y - minY) / 2.54f);

            Quaternion currentWorldRot = child.rotation;

            child.parent = null;
            child.rotation = currentWorldRot;

            if (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height)
            {
                while (gridX >= 0 && gridX < width && gridY >= 0 && gridY < height && grid[gridX, gridY] != null)
                {
                    gridY++;
                }

                if (gridY < height)
                {
                    child.position = new Vector3(minX + (gridX * 2.54f), minY + (gridY * 2.54f), child.position.z);
                    grid[gridX, gridY] = child;
                }
            }
        }

        AudioClip clickClip = CreateProceduralSound(150f, 0.05f, 25f);
        AudioSource.PlayClipAtPoint(clickClip, Vector3.zero, 0.5f);

        CheckForLines();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10, transform.position);
        }

        this.enabled = false;

        BlockSpawner spawner = FindObjectOfType<BlockSpawner>();
        if (spawner != null)
        {
            spawner.NewRandomPiece();
        }

        Destroy(gameObject);
    }

    public static void CheckForLines()
    {
        int clearedLines = 0;

        for (int y = 0; y < height; y++)
        {
            if (HasLine(y))
            {
                DeleteLine(y);
                RowDown(y);
                y--;
                clearedLines++;
            }
        }

        if (clearedLines > 0)
        {
            ShowComboText(clearedLines);
        }
    }

    static void ShowComboText(int count)
    {
        string message = count switch
        {
            1 => "Good!",
            2 => "Double!",
            3 => "Triple!",
            _ => "TETRIS!"
        };

        GameObject prefab = Resources.Load<GameObject>("PopupTextPrefab");
        if (prefab == null) return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            GameObject popupObj = Instantiate(prefab, canvas.transform, false);

            RectTransform rect = popupObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, 50);
                rect.localScale = Vector3.one;
            }

            PopupText popup = popupObj.GetComponent<PopupText>();
            if (popup != null)
            {
                popup.Initialize(message);
            }
        }
    }

    static bool HasLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    static void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }

        AudioClip clearClip = CreateProceduralSound(550f, 0.15f, 5f);
        AudioSource.PlayClipAtPoint(clearClip, Vector3.zero, 0.7f);
    }

    static void RowDown(int startIndex)
    {
        for (int y = startIndex; y < height - 1; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y] = grid[x, y + 1];
                if (grid[x, y] != null)
                {
                    Vector3 currentPos = grid[x, y].position;
                    int newGridY = y;

                    TetrisPiece activeScript = FindObjectOfType<TetrisPiece>();
                    float currentMinX = activeScript != null ? activeScript.minX : -14.28f;
                    float currentMinY = activeScript != null ? activeScript.minY : -23f;

                    int gridX = Mathf.RoundToInt((currentPos.x - currentMinX) / 2.54f);
                    grid[x, y].position = new Vector3(currentMinX + (gridX * 2.54f), currentMinY + (newGridY * 2.54f), currentPos.z);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            grid[x, height - 1] = null;
        }
    }

    public static void ResetGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = null;
            }
        }
    }

    static AudioClip CreateProceduralSound(float frequency, float duration, float decay)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * Mathf.Exp(-decay * t);
        }

        AudioClip clip = AudioClip.Create("ProceduralSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}