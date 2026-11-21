
using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    [Header("Background Sprites")]
    public Transform background1;
    public Transform background2;

    [Header("Reposition Settings")]
    public float backgroundHeight = 20f;

    public float recycleY = -15f;

    [Header("Speed")]
    public float baseSpeed = 0.15f;
    public float maximumSpeed = 0.5f;

    private float currentSpeed;

    private void Start()
    {
        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        MoveBackground(background1);
        MoveBackground(background2);

        CheckBackgroundPosition();
    }

    private void MoveBackground(Transform bg)
    {
        if (bg == null)
            return;

        bg.position += Vector3.down *
                       currentSpeed *
                       Time.deltaTime;
    }

    private void CheckBackgroundPosition()
    {
        if (background1 == null || background2 == null)
            return;

        // Background 1 reaches the bottom.
        if (background1.position.y <= recycleY)
        {
            background1.position = new Vector3(
                background1.position.x,
                background2.position.y + backgroundHeight,
                background1.position.z
            );
        }

        // Background 2 reaches the bottom.
        if (background2.position.y <= recycleY)
        {
            background2.position = new Vector3(
                background2.position.x,
                background1.position.y + backgroundHeight,
                background2.position.z
            );
        }
    }

    public void SetBackgroundSpeed(float difficulty)
    {
        currentSpeed = Mathf.Lerp(
            baseSpeed,
            maximumSpeed,
            Mathf.Clamp01(difficulty)
        );
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }
}
