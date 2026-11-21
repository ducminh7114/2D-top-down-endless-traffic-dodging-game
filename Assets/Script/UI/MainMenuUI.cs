
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "InGame";

    [Header("UI Document")]
    public UIDocument uiDocument;

    private Label highScoreText;

    private Button startButton;
    private Button settingsButton;
    private Button quitButton;
    private Button backButton;

    private VisualElement mainMenuPanel;
    private VisualElement settingsPanel;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("MainMenuManager: UIDocument not assigned!");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        // Get UI elements
        highScoreText = root.Q<Label>("HighScoreText");

        startButton = root.Q<Button>("StartButton");
        settingsButton = root.Q<Button>("SettingsButton");
        quitButton = root.Q<Button>("QuitButton");
        backButton = root.Q<Button>("BackButton");

        mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        settingsPanel = root.Q<VisualElement>("SettingsPanel");

        // Register button events
        if (startButton != null)
            startButton.clicked += StartGame;

        if (settingsButton != null)
            settingsButton.clicked += OpenSettings;

        if (quitButton != null)
            quitButton.clicked += QuitGame;

        if (backButton != null)
            backButton.clicked += CloseSettings;

        // Initial UI setup
        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.None;

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.Flex;

        UpdateHighScore();
    }

    private void OnDisable()
    {
        if (startButton != null)
            startButton.clicked -= StartGame;

        if (settingsButton != null)
            settingsButton.clicked -= OpenSettings;

        if (quitButton != null)
            quitButton.clicked -= QuitGame;

        if (backButton != null)
            backButton.clicked -= CloseSettings;
    }

    private void UpdateHighScore()
    {
        int highScore = PlayerPrefs.GetInt("TrafficHighScore", 0);

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + highScore;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.None;

        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.Flex;
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.None;

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");

        Application.Quit();
    }
}
