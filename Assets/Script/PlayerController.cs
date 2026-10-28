using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject playerPrefab; // Assign player prefab in the Unity editor
    public GameObject powerUpPrefab; // Assign power-up prefab in the Unity editor
    public ScoreCounter scoreCounter; // Reference to the ScoreCounter script
    public GameObject gameOverObject; // Assign the Game Over GameObject in the Unity editor
    
   

    private void PlayerDeath()
    {
        // Your logic for handling player death

        // Call the method to reset the score
        scoreCounter.ResetScoreOnPlayerDeath();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PowerUp")) // Check if the collided object is a power-up
        {
            Destroy(other.gameObject); // Destroy the power-up object

            if (scoreCounter != null) // Check if the ScoreCounter script is assigned
            {
                scoreCounter.IncreaseScore(20); // Increase the score by 20 using the ScoreCounter script
            }

            Debug.Log("Power-up collected!");
        }
        else if (other.gameObject.CompareTag("Obstacle")) // Check if the collided object is an obstacle
        {
            Destroy(playerPrefab); // Destroy the player object
            Debug.Log("Player destroyed by obstacle!");
            // Implement game over logic or any other required actions

            if (gameOverObject != null)
            {
                gameOverObject.SetActive(true); // Turn on the Game Over GameObject
            }
        }
    }

    private void Start()
    {
        // Assuming your ScoreCounter is on the same GameObject as this script
        scoreCounter = GetComponent<ScoreCounter>();
        
        if (scoreCounter != null) // Check if the ScoreCounter script is assigned
        {
            scoreCounter.IncreaseScore(1); // Increase the score by 1 using the ScoreCounter script
        }
    }

    public void DestroyPlayer()
    {
        Destroy(gameObject); // Destroy the player object

        if (gameOverObject != null)
        {
            gameOverObject.SetActive(true); // Turn on the Game Over GameObject
        }
    }
}