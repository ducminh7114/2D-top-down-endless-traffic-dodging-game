
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

    public float baseSpawnInterval = 2.5f;
    public float minimumSpawnInterval = 0.8f;

    [Range(0f, 1f)]
    public float baseUniqueCarChance = 0.05f;

    [Range(0f, 1f)]
    public float maximumUniqueCarChance = 0.30f;

    public float scoreForMaximumDifficulty = 1000f;

    [Header("References")]
    public TrafficSpawner trafficSpawner;
    public ScrollingBG scrollingBG;

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

        highScore = PlayerPrefs.GetInt("TrafficHighScore", 0);
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
        scoreText = root.Q<Label>("ScoreText");
        highScoreText = root.Q<Label>("HighScoreText");

        gameOverScoreText = root.Q<Label>("GameOverScoreText");
        gameOverHighScoreText = root.Q<Label>("GameOverHighScoreText");

        gameOverPanel = root.Q<VisualElement>("GameOverPanel");

        restartButton = root.Q<Button>("RestartButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");

        if (restartButton != null)
            restartButton.clicked += RestartGame;

        if (mainMenuButton != null)
            mainMenuButton.clicked += ReturnToMainMenu;

        if (gameOverPanel != null)
            gameOverPanel.style.display = DisplayStyle.None;

        UpdateUI();
    }

    private void OnDisable()
    {
        if (restartButton != null)
            restartButton.clicked -= RestartGame;

        if (mainMenuButton != null)
            mainMenuButton.clicked -= ReturnToMainMenu;
    }

    private void Start()
    {
        if (currentState == GameState.MainMenu)
            return;

        StartGame();
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
            currentScore / scoreForMaximumDifficulty
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
            highScoreText.text = "HIGH SCORE: " + highScore;
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
}
