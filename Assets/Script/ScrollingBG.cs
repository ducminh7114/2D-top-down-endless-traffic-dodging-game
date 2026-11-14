
using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    [Header("Background")]
    public Renderer bgRenderer;

    [Header("Speed")]
    public float baseSpeed = 0.15f;
    public float maximumSpeed = 0.5f;

    private float currentSpeed;
    private float offset;

    private Material bgMaterial;

    private void Start()
    {
        if (bgRenderer == null)
            bgRenderer = GetComponent<Renderer>();

        if (bgRenderer != null)
            bgMaterial = bgRenderer.material;

        currentSpeed = baseSpeed;
    }

    private void Update()
    {
        if (bgMaterial == null)
            return;

        offset += currentSpeed * Time.deltaTime;

        bgMaterial.mainTextureOffset =
            new Vector2(0f, offset);
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
