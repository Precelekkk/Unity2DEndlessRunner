using UnityEngine;
using System.Collections.Generic;

public class SpawnManagerForPowerUps : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float minX = 10f;
    public float maxX = 15f;
    public float minDistanceBetweenSpawns = 2f;
    public int numberOfPrefabs = 10;

    private List<Vector3> spawnPositions = new List<Vector3>();

    void Start()
    {
        if (prefabToSpawn == null) return; // Nothing assigned in PlaySec - Instantiate(null) used to throw on every run

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

        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
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