using UnityEngine;
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

    [Header("Turing Light")]
    public Light RightHeadlight;
    public Light LeftHeadlight;
    public float blinkInterval;

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

    private bool turningLightStarted = false;

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
        initialized = true;

        // Determine rotation direction.
        if (targetX > startingX)
            targetRotation = -turnAngle;
        else if (targetX < startingX)
            targetRotation = turnAngle;
        else
            targetRotation = 0f;


        if (LeftHeadlight == null)
        {
            LeftHeadlight = GetComponent<Light>();
        }

        if (RightHeadlight == null)
        {
            LeftHeadlight = GetComponent<Light>();
        }

        // Assign RightHeadlight separately in Inspector.
    }

    private void StartTurningLight()
    {
        Light selectedLight = null;

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

        // Move traffic downward.
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

            float direction = Mathf.Sign(targetX - startingX);

            float targetAngle = -turnAngle * direction * (1f - smoothT);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, targetAngle),
                rotationSpeed * Time.deltaTime
            );
        }

        // Apply movement.
        transform.position = position;

        // Remove vehicles outside the screen.
        if (transform.position.y <= despawnY)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator BlinkLight(Light targetLight)
    {
        if (targetLight == null)
        {
            yield break;
        }

        float timer = 0f;
        bool lightOn = false;

        while (timer < laneChangeDuration)
        {
            lightOn = true;
            targetLight.enabled = lightOn;

            yield return new WaitForSeconds(blinkInterval);

            timer += blinkInterval;
        }

        targetLight.enabled = false;
    }
}