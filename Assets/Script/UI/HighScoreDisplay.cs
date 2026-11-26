
using UnityEngine;
using UnityEngine.UIElements;

public class HighScoreDisplay : MonoBehaviour
{
     public Label highScoreText;
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        highScoreText = root.Q<Label>("HighScoreText");
    }

    private void Start()
    {
        int highScore = PlayerPrefs.GetInt("TrafficHighScore", 0);

        highScoreText.text = "HIGH SCORE: " + highScore;
    }
}

