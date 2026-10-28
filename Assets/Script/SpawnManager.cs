using UnityEngine;
using System.Collections.Generic;

// Endless spawning: keeps the road in front of the cat filled with obstacle
// patterns and power-ups and removes the ones the cat has already passed.
//
// The patterns are built around the triple jump (jump / air jump / slam):
//   Single, Double       - jump over it
//   LowDrone             - flies at head height: stay on the ground and run under it
//   HighDrone            - only in the way of a double jump
//   WideDouble           - two obstacles far apart: jump + air jump (or land in between)
//   JumpThenDrone        - jump the obstacle, then slam down under the drone
// Harder patterns unlock the longer the run lasts.
public class SpawnManager : MonoBehaviour
{
    public GameObject prefabToSpawn;   // Ground obstacle
    public GameObject flyingPrefab;    // Flying enemy ("drone")
    public GameObject speedPrefab;
    public GameObject powerupPrefab;

    [Header("Spacing")]
    public float startDistance = 35f;              // Free road at the start (time to read the tutorial)
    public float minDistanceBetweenSpawns = 6f;
    public float maxDistanceBetweenSpawns = 16f;
    public float spawnAhead = 40f;                 // Spawn this far in front of the player (off screen)
    public float despawnBehind = 25f;              // Remove objects this far behind the player

    [Header("Testing")]
    public bool testAllPatterns = false;           // Dev tool: speed power-up, +20 and every pattern in a row from the start
    public float testStartDistance = 15f;

    [Header("Difficulty (seconds of the run)")]
    public float dronesFrom = 15f;
    public float combosFrom = 40f;

    [Header("Drones")]
    public float lowDroneY = 2.0f;                 // Above a running cat, in the way of a jumping one
    public float highDroneY = 5.2f;                // Only in the way of a double jump
    public float droneBob = 0.25f;                 // How far it floats up and down
    public float droneSpeed = 1.5f;                // Flies slowly towards the cat
    public Color droneColor = new Color(1f, 0.32f, 0.38f);
    public float droneHitbox = 0.8f;               // Collider size (the picture is 1 x 1)

    [Header("Chances")]
    public float speedPrefabSpawnChance = 0.07f; // Chance per spawn slot (one slot every ~1-2 s)
    public float powerupPrefabSpawnChance = 0.15f; // Chance of the +20 points pick-up per slot

    private enum Pattern { Single, Double, LowDrone, HighDrone, WideDouble, JumpThenDrone }

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Transform player;
    private Movement playerMovement;
    private float nextSpawnX;
    private float runTime;
    private int testPatternIndex;
    private static Sprite[] droneFrames;

    void Start()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
            playerMovement = playerController.GetComponent<Movement>();
            nextSpawnX = player.position.x + (testAllPatterns ? testStartDistance : startDistance);
        }
    }

    void Update()
    {
        if (player == null) return; // Player is dead - stop spawning

        runTime += Time.deltaTime;

        while (nextSpawnX < player.position.x + spawnAhead)
        {
            float width = SpawnNext(nextSpawnX);

            // At higher speed the cat covers more ground per jump, so give it more room
            float speedFactor = Mathf.Sqrt(SpeedMultiplier);
            nextSpawnX += width + Random.Range(minDistanceBetweenSpawns, maxDistanceBetweenSpawns) * speedFactor;
        }

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            // Null = already destroyed (e.g. a collected power-up)
            if (spawned[i] == null || spawned[i].transform.position.x < player.position.x - despawnBehind)
            {
                if (spawned[i] != null) Destroy(spawned[i]);
                spawned.RemoveAt(i);
            }
        }
    }

    private float SpeedMultiplier => playerMovement != null ? playerMovement.SpeedMultiplier : 1f;
    private float CurrentSpeed => playerMovement != null ? playerMovement.CurrentSpeed : 8f;

    // Spawns a power-up or an obstacle pattern at x. Returns how long it is on the X axis.
    float SpawnNext(float x)
    {
        if (testAllPatterns)
        {
            // A speed power-up, a +20 pick-up, then each pattern once in order, then start again
            Pattern[] all = (Pattern[])System.Enum.GetValues(typeof(Pattern));
            int step = testPatternIndex++ % (all.Length + 2);
            if (step < 2)
            {
                Spawn(step == 0 ? speedPrefab : powerupPrefab, x, transform.position.y);
                return 0f;
            }
            return SpawnPattern(all[step - 2], x);
        }

        float spawnChance = Random.value;
        if (spawnChance < speedPrefabSpawnChance)
        {
            Spawn(speedPrefab, x, transform.position.y);
            return 0f;
        }
        if (spawnChance < speedPrefabSpawnChance + powerupPrefabSpawnChance)
        {
            Spawn(powerupPrefab, x, transform.position.y);
            return 0f;
        }

        return SpawnPattern(PickPattern(), x);
    }

    Pattern PickPattern()
    {
        // Weighted list of what is allowed at this point of the run
        List<(Pattern pattern, float weight)> options = new List<(Pattern, float)>
        {
            (Pattern.Single, 5f),
            (Pattern.Double, 3f),
        };

        if (flyingPrefab != null && runTime >= dronesFrom)
        {
            options.Add((Pattern.LowDrone, 2f));
            options.Add((Pattern.HighDrone, 1f));
        }

        if (runTime >= combosFrom)
        {
            options.Add((Pattern.WideDouble, 2f));
            if (flyingPrefab != null) options.Add((Pattern.JumpThenDrone, 2f));
        }

        float total = 0f;
        foreach (var option in options) total += option.weight;

        float roll = Random.value * total;
        foreach (var option in options)
        {
            roll -= option.weight;
            if (roll <= 0f) return option.pattern;
        }
        return Pattern.Single;
    }

    float SpawnPattern(Pattern pattern, float x)
    {
        float groundY = transform.position.y;

        switch (pattern)
        {
            case Pattern.Double:
                Spawn(prefabToSpawn, x, groundY);
                Spawn(prefabToSpawn, x + 1.6f, groundY); // Close enough to clear both with one jump
                return 1.6f;

            case Pattern.LowDrone:
                SpawnDrone(x, lowDroneY);
                return 0f;

            case Pattern.HighDrone:
                SpawnDrone(x, highDroneY);
                Spawn(prefabToSpawn, x, groundY); // Jump it, but do not double jump into the drone
                return 0f;

            case Pattern.WideDouble:
            {
                // Two obstacles (never more - a long wall was no fun to jump) far enough apart
                // that one jump + the air jump is the easy way; landing in between also works
                float gap = Mathf.Max(3f, CurrentSpeed * 0.55f);
                Spawn(prefabToSpawn, x, groundY);
                Spawn(prefabToSpawn, x + gap, groundY);
                return gap;
            }

            case Pattern.JumpThenDrone:
            {
                // The drone hangs where the cat is still in the air after the jump - slam to get under it
                float droneOffset = CurrentSpeed * 0.45f;
                Spawn(prefabToSpawn, x, groundY);
                SpawnDrone(x + droneOffset, lowDroneY);
                return droneOffset;
            }

            default: // Single
                Spawn(prefabToSpawn, x, groundY);
                return 0f;
        }
    }

    void SpawnDrone(float x, float y)
    {
        GameObject drone = Spawn(flyingPrefab, x, y);
        if (drone == null) return;

        EnemyMovement movement = drone.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = droneSpeed;
            movement.minY = y - droneBob;
            movement.maxY = y + droneBob;
        }

        // The Circle prefab points to a sprite that is no longer in the project, so the
        // drone was invisible (but still deadly). It gets its own pixel art instead:
        // a red bird flapping its wings.
        SpriteRenderer sprite = drone.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            if (droneFrames == null) droneFrames = DroneArt.CreateFrames(droneColor);
            sprite.sprite = droneFrames[0];
            sprite.color = Color.white;

            SpriteFlap flap = drone.AddComponent<SpriteFlap>();
            flap.frames = droneFrames;
        }

        // Hitbox a bit smaller than the picture - grazing the edge should not kill
        BoxCollider2D box = drone.GetComponent<BoxCollider2D>();
        if (box != null) box.size = new Vector2(droneHitbox, droneHitbox);
    }

    GameObject Spawn(GameObject prefab, float x, float y)
    {
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, new Vector3(x, y, transform.position.z), Quaternion.identity);

        // Always in front of the background
        foreach (SpriteRenderer renderer in instance.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 5);
        }

        spawned.Add(instance);
        return instance;
    }
}
