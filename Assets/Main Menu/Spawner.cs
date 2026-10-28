using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public GameObject spawnAreaObject;
    public float minSpawnInterval = 1f; // Updated to 1 second
    public float maxSpawnInterval = 1f; // Updated to 1 second

    private void Start()
    {
        Invoke("SpawnObject", Random.Range(minSpawnInterval, maxSpawnInterval));
    }

    private void SpawnObject()
    {
        if (spawnAreaObject == null)
        {
            Debug.LogError("Spawn area object not set!");
            return;
        }

        if (objectToSpawn == null)
        {
            Debug.LogError("Object to spawn not set!");
            return;
        }

        Collider2D spawnCollider = spawnAreaObject.GetComponent<Collider2D>();

        if (spawnCollider == null)
        {
            Debug.LogError("Collider not found on spawn area object!");
            return;
        }

        Vector3 spawnPoint = new Vector3(
            Random.Range(spawnAreaObject.transform.position.x - spawnCollider.bounds.extents.x, spawnAreaObject.transform.position.x + spawnCollider.bounds.extents.x),
            Random.Range(spawnAreaObject.transform.position.y - spawnCollider.bounds.extents.y, spawnAreaObject.transform.position.y + spawnCollider.bounds.extents.y),
            spawnAreaObject.transform.position.z
        );

        GameObject newObject = Instantiate(objectToSpawn, spawnPoint, Quaternion.identity);
        Debug.Log("Object spawned at: " + spawnPoint);

        float nextSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke("SpawnObject", nextSpawnInterval);
    }
}
