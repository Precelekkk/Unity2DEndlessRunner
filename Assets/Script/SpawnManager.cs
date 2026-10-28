using UnityEngine;
using System.Collections.Generic;

// Endless spawning: keeps the road in front of the cat filled with obstacles and
// power-ups and removes the ones the cat has already passed.
public class SpawnManager : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject speedPrefab;
    public GameObject powerupPrefab;

    [Header("Spacing")]
    public float minX = 10f;                       // X of the first spawn
    public float minDistanceBetweenSpawns = 6f;
    public float maxDistanceBetweenSpawns = 16f;
    public float spawnAhead = 40f;                 // Spawn this far in front of the player (off screen)
    public float despawnBehind = 25f;              // Remove objects this far behind the player

    [Header("Chances")]
    public float speedPrefabSpawnChance = 0.2f; // Probability of spawning speed prefab
    public float powerupPrefabSpawnChance = 0.1f; // Probability of spawning powerup prefab

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Transform player;
    private Movement playerMovement;
    private float nextSpawnX;

    void Start()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
            playerMovement = playerController.GetComponent<Movement>();
        }

        nextSpawnX = minX;
    }

    void Update()
    {
        if (player == null) return; // Player is dead - stop spawning

        while (nextSpawnX < player.position.x + spawnAhead)
        {
            SpawnPrefab(nextSpawnX);

            // At higher speed the cat covers more ground per jump, so give it more room
            float speedFactor = playerMovement != null ? Mathf.Sqrt(playerMovement.SpeedMultiplier) : 1f;
            nextSpawnX += Random.Range(minDistanceBetweenSpawns, maxDistanceBetweenSpawns) * speedFactor;
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

    void SpawnPrefab(float x)
    {
        Vector3 spawnPosition = new Vector3(x, transform.position.y, transform.position.z);

        // Determine which prefab to spawn based on spawn chances
        GameObject prefab = prefabToSpawn;
        float spawnChance = Random.value;
        if (spawnChance < speedPrefabSpawnChance)
        {
            prefab = speedPrefab;
        }
        else if (spawnChance < speedPrefabSpawnChance + powerupPrefabSpawnChance)
        {
            prefab = powerupPrefab;
        }

        if (prefab != null)
        {
            spawned.Add(Instantiate(prefab, spawnPosition, Quaternion.identity));
        }
    }
}
