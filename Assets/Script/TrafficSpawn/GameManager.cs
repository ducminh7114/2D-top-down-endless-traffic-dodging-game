
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver
    }

    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Difficulty")]
    public float scorePerSecond = 10f;
    public float baseTrafficSpeed = 8f;
    public float maximumTrafficSpeed = 16f;

    public float baseSpawnInterval = 0.5f;
    public float minimumSpawnInterval = 0.1f;

    [Range(0f, 1f)]
    public float baseUniqueCarChance = 0.05f;

    [Range(0f, 1f)]
    public float maximumUniqueCarChance = 0.30f;

    public float scoreForMaximumDifficulty = 1000f;

    [Header("References")]
    public TrafficSpawner trafficSpawner;
    public ScrollingBG scrollingBG;
    public DayNightManager dayNightManager;

    [Header("Coin System")]
    public int totalCoins { get; private set; }

    private const string HIGH_SCORE_KEY = "TrafficHighScore";
    private const string COINS_KEY = "TrafficCoins";

    private Label coinText;
    private Button deleteAllDataButton;

    [Header("UI Document")]
    public UIDocument uiDocument;

    private Label scoreText;
    private Label highScoreText;
    private Label gameOverScoreText;
    private Label gameOverHighScoreText;

    private VisualElement gameOverPanel;

    private Button restartButton;
    private Button mainMenuButton;

    private float scoreTimer;

    public int currentScore { get; private set; }
    public int highScore { get; private set; }

    public float Difficulty01 { get; private set; }
    public float CurrentTrafficSpeed { get; private set; }
    public float CurrentSpawnInterval { get; private set; }
    public float UniqueCarChance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadPlayerData();
    }

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("GameManager: UIDocument not assigned!");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        // Retrieve UXML elements by their names.
        coinText = root.Q<Label>("CoinText");
        scoreText = root.Q<Label>("ScoreText");
        highScoreText = root.Q<Label>("HighScoreText");

        gameOverScoreText = root.Q<Label>("GameOverScoreText");
        gameOverHighScoreText = root.Q<Label>("GameOverHighScoreText");

        gameOverPanel = root.Q<VisualElement>("gameOverPanel");

        restartButton = root.Q<Button>("RestartButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");

        if (restartButton != null)
            restartButton.clicked += RestartGame;

        if (mainMenuButton != null)
            mainMenuButton.clicked += ReturnToMainMenu;

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.None;

        if (deleteAllDataButton != null)
            deleteAllDataButton.clicked += DeleteAllData;
        
        UpdateUI();
    }

    private void OnDisable()
    {
        if (restartButton != null)
            restartButton.clicked -= RestartGame;

        if (mainMenuButton != null)
            mainMenuButton.clicked -= ReturnToMainMenu;

        if (deleteAllDataButton != null)
            deleteAllDataButton.clicked -= DeleteAllData;
    }

    private void Start()
    {
        if (currentState == GameState.MainMenu)
            return;

        StartGame();
    }

    private void LoadPlayerData()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);

        PlayerPrefs.Save();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        totalCoins += amount;

        SavePlayerData();
        UpdateUI();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        scoreTimer += Time.deltaTime;

        while (scoreTimer >= 1f)
        {
            scoreTimer -= 1f;

            currentScore += Mathf.RoundToInt(scorePerSecond);

            UpdateDifficulty();
            UpdateUI();
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        scoreTimer = 0f;

        currentState = GameState.Playing;

        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.None;

        UpdateDifficulty();
        UpdateUI();
    }

    private void UpdateDifficulty()
    {
        Difficulty01 = Mathf.Clamp01(
            currentScore / Mathf.Max(1f, scoreForMaximumDifficulty)
        );

        CurrentTrafficSpeed = Mathf.Lerp(
            baseTrafficSpeed,
            maximumTrafficSpeed,
            Difficulty01
        );

        CurrentSpawnInterval = Mathf.Lerp(
            baseSpawnInterval,
            minimumSpawnInterval,
            Difficulty01
        );

        UniqueCarChance = Mathf.Lerp(
            baseUniqueCarChance,
            maximumUniqueCarChance,
            Difficulty01
        );

        if (trafficSpawner != null)
        {
            trafficSpawner.SetDifficulty(
                CurrentTrafficSpeed,
                CurrentSpawnInterval,
                UniqueCarChance
            );
        }

        if (scrollingBG != null)
        {
            scrollingBG.SetBackgroundSpeed(Difficulty01);
        }

        // Update day/night environment based on current score.
        if (dayNightManager != null)
        {
            dayNightManager.CheckScore(currentScore);
        }
    }

    public void GameOver()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.GameOver;

        if (currentScore > highScore)
        {
            highScore = currentScore;

            PlayerPrefs.SetInt("TrafficHighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.Flex;

        if (gameOverScoreText != null)
            gameOverScoreText.text = currentScore.ToString();

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = highScore.ToString();

        Time.timeScale = 0f;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();

        if (highScoreText != null)
            highScoreText.text = " " + highScore;

        if (coinText != null)
            coinText.text = totalCoins.ToString();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void DeleteAllData()
    {
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        PlayerPrefs.DeleteKey(COINS_KEY);

        highScore = 0;
        totalCoins = 0;
        currentScore = 0;

        PlayerPrefs.Save();

        UpdateUI();

        Debug.LogError("ALL DATA HAS BEEN DELETED");
    }
}
