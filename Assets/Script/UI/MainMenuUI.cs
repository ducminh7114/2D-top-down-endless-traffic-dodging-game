
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    public UIDocument uiDocument;

    private Label highScoreText;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        highScoreText = uiDocument.rootVisualElement
            .Q<Label>("HighScoreText");

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt(
                "TrafficHighScore", 0
            );

            highScoreText.text = "HIGH SCORE: " + highScore;
        }
    }
}

