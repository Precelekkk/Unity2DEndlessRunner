using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneChange : MonoBehaviour
{
    public string targetScene = "Play1";
    public string playerTag = "Player"; // Tag to identify the player
    public Transform spawnPoint; // Reference to the spawn point in the new scene

    private GameObject playerPrefab; // Reference to the player prefab

    private void Start()
    {
        // Find the player prefab in the scene
        playerPrefab = GameObject.FindGameObjectWithTag(playerTag);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered the trigger.");

            // Save the current score before changing scenes
            ScoreCounter scoreCounter = FindObjectOfType<ScoreCounter>();
            if (scoreCounter != null)
            {
                scoreCounter.SaveScore(); // Save the score
            }
            else
            {
                Debug.LogWarning("ScoreCounter not found in the scene.");
            }

            // Save player position
            PlayerPrefs.SetFloat("PlayerX", other.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", other.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", other.transform.position.z);

            // Don't destroy this GameObject when loading new scenes
            DontDestroyOnLoad(gameObject);

            // Change the scene to the target scene
            SceneManager.LoadScene(targetScene);
        }
    }

    // Subscribe to the sceneLoaded event when this script is enabled
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unsubscribe from the sceneLoaded event when this script is disabled
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called after a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetScene)
        {
            // Spawn the player at the specified spawn point
            if (spawnPoint != null && playerPrefab != null)
            {
                // Destroy the player GameObject if it exists
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                {
                    Destroy(player);
                }

                // Instantiate the player at the spawn point
                Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}