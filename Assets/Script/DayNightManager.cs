
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightManager : MonoBehaviour
{
    public enum EnvironmentState
    {
        Day,
        Night
    }

    [Header("Score Settings")]
    [Min(1)]
    public int scoreInterval = 200;

    public bool startAtDay = true;

    [Header("Global 2D Light")]
    public Light2D globalLight;

    [Header("Day Settings")]
    public Color dayColor = Color.white;
    [Range(0f, 1f)]
    public float dayIntensity = 1f;

    [Header("Night Settings")]
    public Color nightColor = new Color(0.25f, 0.35f, 0.65f);
    [Range(0f, 1f)]
    public float nightIntensity = 0.25f;

    [Header("Background")]
    public SpriteRenderer backgroundRenderer;

    public Color dayBackgroundColor = Color.white;
    public Color nightBackgroundColor = new Color(0.3f, 0.35f, 0.5f);

    [Header("Transition")]
    [Min(0f)]
    public float transitionDuration = 2f;

    public EnvironmentState CurrentEnvironment { get; private set; }

    private int lastMilestone = -1;

    private float transitionTimer;
    private bool isTransitioning;

    private Color startLightColor;
    private Color targetLightColor;

    private float startLightIntensity;
    private float targetLightIntensity;

    private Color startBackgroundColor;
    private Color targetBackgroundColor;

    private void Start()
    {
        lastMilestone = 0;

        SetEnvironment(
            startAtDay
                ? EnvironmentState.Day
                : EnvironmentState.Night,
            true
        );
    }

    private void Update()
    {
        if (isTransitioning)
            UpdateTransition();
    }

    // Called by GameManager whenever score changes.
    public void CheckScore(int score)
    {
        int milestone = score / Mathf.Max(1, scoreInterval);

        if (milestone == lastMilestone)
            return;

        lastMilestone = milestone;

        // Every milestone alternates between day and night.
        bool isDay = milestone % 2 == 0;

        SetEnvironment(
            isDay
                ? EnvironmentState.Day
                : EnvironmentState.Night
        );
    }

    public void SetEnvironment(
        EnvironmentState state,
        bool instant = false)
    {
        CurrentEnvironment = state;

        if (state == EnvironmentState.Day)
        {
            targetLightColor = dayColor;
            targetLightIntensity = dayIntensity;
            targetBackgroundColor = dayBackgroundColor;
        }
        else
        {
            targetLightColor = nightColor;
            targetLightIntensity = nightIntensity;
            targetBackgroundColor = nightBackgroundColor;
        }

        if (globalLight != null)
        {
            startLightColor = globalLight.color;
            startLightIntensity = globalLight.intensity;
        }

        if (backgroundRenderer != null)
        {
            startBackgroundColor = backgroundRenderer.color;
        }

        if (instant || transitionDuration <= 0f)
        {
            ApplyFinalSettings();
            return;
        }

        transitionTimer = 0f;
        isTransitioning = true;
    }

    private void UpdateTransition()
    {
        transitionTimer += Time.deltaTime;

        float t = Mathf.Clamp01(
            transitionTimer / transitionDuration
        );

        // Smooth transition.
        t = Mathf.SmoothStep(0f, 1f, t);

        if (globalLight != null)
        {
            globalLight.color = Color.Lerp(
                startLightColor,
                targetLightColor,
                t
            );

            globalLight.intensity = Mathf.Lerp(
                startLightIntensity,
                targetLightIntensity,
                t
            );
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = Color.Lerp(
                startBackgroundColor,
                targetBackgroundColor,
                t
            );
        }

        if (transitionTimer >= transitionDuration)
        {
            ApplyFinalSettings();
        }
    }

    private void ApplyFinalSettings()
    {
        isTransitioning = false;

        if (globalLight != null)
        {
            globalLight.color = targetLightColor;
            globalLight.intensity = targetLightIntensity;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = targetBackgroundColor;
        }
    }
}

