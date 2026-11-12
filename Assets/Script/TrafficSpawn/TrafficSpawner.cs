
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TrafficSpawnEntry
{
    public string vehicleName;

    public TrafficVehicle prefab;

    [Min(0f)]
    public float spawnWeight = 10f;
}

public class TrafficSpawner : MonoBehaviour
{
    private const int LANE_COUNT = 4;

    [Header("Lane Configuration")]
    [Tooltip("X positions of lanes, ordered left to right.")]
    public float[] laneCentersX = new float[4]
    {
        -3f, -1f, 1f, 3f
    };

    [Header("Spawn Position")]
    public float spawnY = 15f;

    public float despawnY = -15f;

    [Header("Traffic Movement")]
    [Min(0.1f)]
    public float trafficSpeed = 8f;

    [Min(0f)]
    public float minimumGap = 3f;

    [Header("Traffic Generation")]
    [Range(0f, 1f)]
    public float laneFillChance = 0.65f;

    [Range(2, 20)]
    public int lookAheadWaves = 8;

    [Min(1)]
    public int generationAttempts = 100;

    [Header("Route Validation")]
    [Range(1, 3)]
    public int maxLaneChangesBetweenWaves = 1;

    [Range(0, 3)]
    public int initialLaneChangeAllowance = 3;

    [Range(0, 3)]
    public int playerStartingLane = 1;

    [Header("Vehicle List")]
    public List<TrafficSpawnEntry> vehicles =
        new List<TrafficSpawnEntry>();

    private class PlannedVehicle
    {
        public TrafficVehicle prefab;

        public int startLane;
        public int targetLane;

        public bool turning;
    }

    private class WavePlan
    {
        public bool[] occupied = new bool[LANE_COUNT];

        public List<PlannedVehicle> vehicles =
            new List<PlannedVehicle>();
    }

    private readonly List<WavePlan> plannedWaves =
        new List<WavePlan>();

    private float maxVehicleLength = 4f;

    private float spawnInterval;

    private void Start()
    {
        if (laneCentersX == null ||
            laneCentersX.Length != LANE_COUNT)
        {
            Debug.LogError(
                "TrafficSpawner requires exactly 4 lane positions."
            );

            enabled = false;
            return;
        }

        if (vehicles.Count == 0 ||
            !vehicles.Any(v => v.prefab != null && v.spawnWeight > 0f))
        {
            Debug.LogError("TrafficSpawner has no valid vehicle entries.");
            enabled = false;
            return;
        }

        CalculateSpacing();

        GenerateInitialWaves();

        StartCoroutine(SpawnLoop());
    }

    private void CalculateSpacing()
    {
        maxVehicleLength = 1f;

        foreach (TrafficSpawnEntry entry in vehicles)
        {
            if (entry.prefab == null)
                continue;

            maxVehicleLength = Mathf.Max(
                maxVehicleLength,
                entry.prefab.vehicleLength
            );
        }

        // Every wave is separated enough for the longest vehicle.
        spawnInterval =
            (maxVehicleLength + minimumGap) / trafficSpeed;
    }

    private void GenerateInitialWaves()
    {
        plannedWaves.Clear();

        for (int i = 0; i < lookAheadWaves; i++)
        {
            AppendValidWave();
        }

        Debug.Log(
            "Traffic system initialized. Planned waves: " +
            plannedWaves.Count
        );
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (plannedWaves.Count == 0)
            {
                AppendValidWave();
            }

            SpawnNextWave();

            AppendValidWave();
        }
    }

    private void SpawnNextWave()
    {
        if (plannedWaves.Count == 0)
            return;

        WavePlan wave = plannedWaves[0];

        // The center of every vehicle starts at the same Y.
        // Wave spacing accounts for the longest prefab.
        float vehicleY = spawnY + maxVehicleLength * 0.5f;

        foreach (PlannedVehicle planned in wave.vehicles)
        {
            float startX = GetVehicleCenterX(
                planned.startLane,
                planned.prefab.lanesOccupied
            );

            float targetX = GetVehicleCenterX(
                planned.targetLane,
                planned.prefab.lanesOccupied
            );

            Vector3 position = new Vector3(
                startX,
                vehicleY,
                0f
            );

            TrafficVehicle vehicle = Instantiate(
                planned.prefab,
                position,
                Quaternion.identity,
                transform
            );

            vehicle.Initialize(
                trafficSpeed,
                targetX,
                planned.turning,
                despawnY
            );
        }

        plannedWaves.RemoveAt(0);
    }

    private float GetVehicleCenterX(int lane, int width)
    {
        if (width == 2)
        {
            return (
                laneCentersX[lane] +
                laneCentersX[lane + 1]
            ) * 0.5f;
        }

        return laneCentersX[lane];
    }

    private TrafficSpawnEntry GetRandomVehicle()
    {
        float totalWeight = 0f;

        foreach (TrafficSpawnEntry entry in vehicles)
        {
            if (entry.prefab != null)
                totalWeight += Mathf.Max(0f, entry.spawnWeight);
        }

        if (totalWeight <= 0f)
            return null;

        float random = Random.Range(0f, totalWeight);

        foreach (TrafficSpawnEntry entry in vehicles)
        {
            if (entry.prefab == null)
                continue;

            random -= Mathf.Max(0f, entry.spawnWeight);

            if (random <= 0f)
                return entry;
        }

        return null;
    }

    private WavePlan GenerateRandomWave()
    {
        WavePlan wave = new WavePlan();

        // Start from a random lane to avoid fixed patterns.
        int offset = Random.Range(0, LANE_COUNT);

        for (int i = 0; i < LANE_COUNT; i++)
        {
            int lane = (i + offset) % LANE_COUNT;

            if (wave.occupied[lane])
                continue;

            if (Random.value > laneFillChance)
                continue;

            TrafficSpawnEntry entry = GetRandomVehicle();

            if (entry == null)
                continue;

            TrafficVehicle prefab = entry.prefab;

            int width = Mathf.Clamp(
                prefab.lanesOccupied, 1, 2
            );

            bool turning = false;

            int targetLane = lane;

            TrafficTurnDirection direction =
                prefab.turnDirection;

            // Two-lane vehicles cannot turn in this version.
            if (width == 1 &&
                direction != TrafficTurnDirection.None)
            {
                if (direction == TrafficTurnDirection.Random)
                {
                    List<int> possibleDirections =
                        new List<int>();

                    if (lane > 0)
                        possibleDirections.Add(-1);

                    if (lane < LANE_COUNT - 1)
                        possibleDirections.Add(1);

                    if (possibleDirections.Count == 0)
                        continue;

                    targetLane = lane +
                        possibleDirections[
                            Random.Range(
                                0,
                                possibleDirections.Count
                            )
                        ];
                }
                else if (direction == TrafficTurnDirection.Left)
                {
                    targetLane = lane - 1;
                }
                else if (direction == TrafficTurnDirection.Right)
                {
                    targetLane = lane + 1;
                }

                if (targetLane < 0 ||
                    targetLane >= LANE_COUNT)
                    continue;

                // Reserve both lanes throughout the wave.
                if (wave.occupied[targetLane])
                    continue;

                turning = true;
            }

            // Check all occupied lanes before placing.
            bool canPlace = true;

            for (int j = 0; j < width; j++)
            {
                int checkLane = lane + j;

                if (checkLane >= LANE_COUNT ||
                    wave.occupied[checkLane])
                {
                    canPlace = false;
                    break;
                }
            }

            if (turning && wave.occupied[targetLane])
                canPlace = false;

            if (!canPlace)
                continue;

            // Reserve lanes.
            for (int j = 0; j < width; j++)
            {
                wave.occupied[lane + j] = true;
            }

            if (turning)
            {
                wave.occupied[targetLane] = true;
            }

            wave.vehicles.Add(new PlannedVehicle
            {
                prefab = prefab,
                startLane = lane,
                targetLane = targetLane,
                turning = turning
            });
        }

        return wave;
    }

    private void AppendValidWave()
    {
        for (int attempt = 0;
             attempt < generationAttempts;
             attempt++)
        {
            WavePlan candidate = GenerateRandomWave();

            List<WavePlan> test =
                new List<WavePlan>(plannedWaves);

            test.Add(candidate);

            if (HasValidRoute(test))
            {
                plannedWaves.Add(candidate);
                return;
            }
        }

        // Safe fallback: spawn an empty wave rather than
        // generate an impossible traffic pattern.
        WavePlan emptyWave = new WavePlan();

        plannedWaves.Add(emptyWave);

        Debug.LogWarning(
            "No valid traffic pattern found. Added an empty wave."
        );
    }

    // --------------------------------------------------
    // ROUTE VALIDATION
    // --------------------------------------------------

    private bool HasValidRoute(List<WavePlan> waves)
    {
        if (waves.Count == 0)
            return true;

        bool[] reachable = new bool[LANE_COUNT];

        // The player can move between lanes before reaching
        // the first distant traffic wave.
        for (int lane = 0; lane < LANE_COUNT; lane++)
        {
            if (Mathf.Abs(lane - playerStartingLane) <=
                initialLaneChangeAllowance &&
                !waves[0].occupied[lane])
            {
                reachable[lane] = true;
            }
        }

        if (!reachable.Any(x => x))
            return false;

        // Find a continuous path through all future waves.
        for (int waveIndex = 1;
             waveIndex < waves.Count;
             waveIndex++)
        {
            bool[] nextReachable =
                new bool[LANE_COUNT];

            for (int lane = 0; lane < LANE_COUNT; lane++)
            {
                if (waves[waveIndex].occupied[lane])
                    continue;

                for (int previousLane = 0;
                     previousLane < LANE_COUNT;
                     previousLane++)
                {
                    if (!reachable[previousLane])
                        continue;

                    if (Mathf.Abs(
                        lane - previousLane
                    ) <= maxLaneChangesBetweenWaves)
                    {
                        nextReachable[lane] = true;
                        break;
                    }
                }
            }

            reachable = nextReachable;

            if (!reachable.Any(x => x))
                return false;
        }

        return true;
    }

    // --------------------------------------------------
    // PLAYER LANE UPDATE
    // --------------------------------------------------

    public void SetPlayerLane(int lane)
    {
        playerStartingLane = Mathf.Clamp(
            lane, 0, LANE_COUNT - 1
        );
    }

    // --------------------------------------------------
    // DEBUG
    // --------------------------------------------------

    [ContextMenu("Print Planned Traffic")]
    private void PrintPlannedTraffic()
    {
        for (int i = 0; i < plannedWaves.Count; i++)
        {
            string result = "Wave " + i + ": ";

            for (int lane = 0; lane < LANE_COUNT; lane++)
            {
                result += plannedWaves[i].occupied[lane]
                    ? "[X] "
                    : "[ ] ";
            }

            Debug.Log(result);
        }
    }
}
