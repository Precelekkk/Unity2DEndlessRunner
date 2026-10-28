using UnityEngine;

// Main menu decoration: cats keep dropping from above the screen at random places,
// spinning, in random sizes - just for fun. Each copy is removed after a few seconds.
public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;      // The cat to copy (has a Rigidbody, so it falls by itself)
    public float minSpawnInterval = 0.6f;
    public float maxSpawnInterval = 1.8f;

    [Header("Variety")]
    public float minScale = 0.7f;
    public float maxScale = 1.3f;
    public float maxSpin = 180f;          // Degrees per second, left or right
    public float lifetime = 8f;           // Long enough to fall through the whole screen

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        if (objectToSpawn != null)
        {
            objectToSpawn.SetActive(false); // The scene copy is only a template
        }
        Invoke(nameof(SpawnObject), Random.Range(0f, maxSpawnInterval));
    }

    private void SpawnObject()
    {
        if (objectToSpawn != null && cam != null)
        {
            // Anywhere across the visible width, just above the top edge
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            Vector3 spawnPoint = new Vector3(
                Random.Range(center.x - halfWidth, center.x + halfWidth),
                center.y + halfHeight + 1.5f,
                objectToSpawn.transform.position.z);

            GameObject cat = Instantiate(objectToSpawn, spawnPoint, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            cat.transform.localScale = objectToSpawn.transform.localScale * Random.Range(minScale, maxScale);
            cat.SetActive(true);

            // The template has a 3D Rigidbody; spin it around the screen axis
            Rigidbody body = cat.GetComponent<Rigidbody>();
            if (body != null) body.angularVelocity = new Vector3(0f, 0f, Random.Range(-maxSpin, maxSpin) * Mathf.Deg2Rad);
            Rigidbody2D body2D = cat.GetComponent<Rigidbody2D>();
            if (body2D != null) body2D.angularVelocity = Random.Range(-maxSpin, maxSpin);

            Destroy(cat, lifetime);
        }

        Invoke(nameof(SpawnObject), Random.Range(minSpawnInterval, maxSpawnInterval));
    }
}
