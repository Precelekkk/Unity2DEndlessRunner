using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject speedPrefab;
    public GameObject powerupPrefab;
    public float minX = 10f;
    public float maxX = 15f;
    public float minDistanceBetweenSpawns = 2f;
    public int numberOfPrefabs = 10;

    public float speedPrefabSpawnChance = 0.2f; // Probability of spawning speed prefab
    public float powerupPrefabSpawnChance = 0.1f; // Probability of spawning powerup prefab

    private List<Vector3> spawnPositions = new List<Vector3>();

    void Start()
    {
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);

        // Check if the new spawn position is too close to any existing spawn position
        while (IsTooCloseToExistingSpawn(spawnPosition))
        {
            randomX = Random.Range(minX, maxX);
            spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);
        }

        spawnPositions.Add(spawnPosition); // Add the spawn position to the list

        // Determine which prefab to spawn based on spawn chances
        float spawnChance = Random.value;
        if (spawnChance < speedPrefabSpawnChance)
        {
            Instantiate(speedPrefab, spawnPosition, Quaternion.identity);
        }
        else if (spawnChance < speedPrefabSpawnChance + powerupPrefabSpawnChance)
        {
            Instantiate(powerupPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }

    bool IsTooCloseToExistingSpawn(Vector3 position)
    {
        foreach (Vector3 spawnPos in spawnPositions)
        {
            if (Vector3.Distance(position, spawnPos) < minDistanceBetweenSpawns)
            {
                return true;
            }
        }
        return false;
    }
}