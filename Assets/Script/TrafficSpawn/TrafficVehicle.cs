
using UnityEngine;

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

    private float moveSpeed;
    private float despawnY;

    private float startingX;
    private float targetX;

    private float elapsed;
    private float turnElapsed;

    private bool canTurn;
    private bool initialized;

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
    }

    private void Update()
    {
        if (!initialized)
            return;

        elapsed += Time.deltaTime;

        // Move traffic downward.
        Vector3 position = transform.position;

        position.y -= moveSpeed * Time.deltaTime;

        // Perform lane change if this vehicle can turn.
        if (canTurn && elapsed >= laneChangeDelay)
        {
            turnElapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                turnElapsed / laneChangeDuration
            );

            position.x = Mathf.Lerp(
                startingX,
                targetX,
                Mathf.SmoothStep(0f, 1f, t)
            );
        }

        transform.position = position;

        // Remove vehicles outside the screen.
        if (transform.position.y <= despawnY)
        {
            Destroy(gameObject);
        }
    }
}
