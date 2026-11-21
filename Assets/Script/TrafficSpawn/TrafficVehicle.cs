
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public enum TrafficTurnDirection
{
    None,
    Left,
    Right,
    Random
}

public class TrafficVehicle : MonoBehaviour
{
    [Header("Lane Occupancy")]
    [Range(1, 2)]
    public int lanesOccupied = 1;

    [Header("Vehicle Size")]
    [Min(0.5f)]
    public float vehicleLength = 4f;

    [Header("Turning")]
    public TrafficTurnDirection turnDirection =
        TrafficTurnDirection.None;

    [Min(0f)]
    public float laneChangeDelay = 1f;

    [Min(0.1f)]
    public float laneChangeDuration = 1f;

    [Header("Rotation")]
    public float turnAngle = 45f;
    public float rotationSpeed = 5f;

    [Header("Turn Signal Lights (URP 2D)")]
    public Light2D RightHeadlight;
    public Light2D LeftHeadlight;

    [Min(0.05f)]
    public float blinkInterval = 0.3f;

    private float moveSpeed;
    private float despawnY;

    private float startingX;
    private float targetX;

    private float elapsed;
    private float turnElapsed;

    private bool canTurn;
    private bool initialized;

    private float targetRotation;

    private Coroutine blinkCoroutine;
    private bool turningLightStarted;

    public void Initialize(
        float speed,
        float destinationX,
        bool shouldTurn,
        float destroyY)
    {
        moveSpeed = speed;
        targetX = destinationX;
        despawnY = destroyY;

        startingX = transform.position.x;
        canTurn = shouldTurn;

        elapsed = 0f;
        turnElapsed = 0f;

        turningLightStarted = false;
        initialized = true;

        if (targetX > startingX)
            targetRotation = -turnAngle;
        else if (targetX < startingX)
            targetRotation = turnAngle;
        else
            targetRotation = 0f;

        // Turn signals start disabled.
        if (LeftHeadlight != null)
            LeftHeadlight.enabled = false;

        if (RightHeadlight != null)
            RightHeadlight.enabled = false;
    }

    private void StartTurningLight()
    {
        Light2D selectedLight = null;

        switch (turnDirection)
        {
            case TrafficTurnDirection.Left:
                selectedLight = LeftHeadlight;
                break;

            case TrafficTurnDirection.Right:
                selectedLight = RightHeadlight;
                break;

            case TrafficTurnDirection.Random:
                selectedLight = Random.value > 0.5f
                    ? LeftHeadlight
                    : RightHeadlight;
                break;
        }

        if (selectedLight != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(
                BlinkLight(selectedLight)
            );
        }
    }

    private void Update()
    {
        if (!initialized)
            return;

        elapsed += Time.deltaTime;

        Vector3 position = transform.position;

        // Move vehicle downward.
        position.y -= moveSpeed * Time.deltaTime;

        if (canTurn && elapsed >= laneChangeDelay)
        {
            turnElapsed += Time.deltaTime;

            if (!turningLightStarted)
            {
                turningLightStarted = true;
                StartTurningLight();
            }

            float t = Mathf.Clamp01(
                turnElapsed / laneChangeDuration
            );

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            position.x = Mathf.Lerp(
                startingX,
                targetX,
                smoothT
            );

            float direction = Mathf.Sign(
                targetX - startingX
            );

            float targetAngle =
                -turnAngle * direction * (1f - smoothT);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, targetAngle),
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = position;

        if (transform.position.y <= despawnY)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator BlinkLight(Light2D targetLight)
    {
        if (targetLight == null)
            yield break;

        float timer = 0f;
        bool lightOn = false;

        float interval = Mathf.Max(0.05f, blinkInterval);

        while (timer < laneChangeDuration)
        {
            lightOn = !lightOn;
            targetLight.enabled = lightOn;

            yield return new WaitForSeconds(interval);

            timer += interval;
        }

        targetLight.enabled = false;
        blinkCoroutine = null;
    }

    private void OnDestroy()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }
}
