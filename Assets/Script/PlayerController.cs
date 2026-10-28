using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject playerPrefab; // Assign player prefab in the Unity editor
    public GameObject powerUpPrefab; // Assign power-up prefab in the Unity editor
    public ScoreCounter scoreCounter; // Reference to the ScoreCounter script
    public GameObject gameOverObject; // Assign the Game Over GameObject in the Unity editor

    [Header("Death feel")]
    public float hitStopDuration = 0.15f; // The game freezes for a moment on the hit
    public float shakeDuration = 0.35f;
    public float shakeStrength = 0.4f;

    private bool isDead = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PowerUp")) // Check if the collided object is a power-up
        {
            Destroy(other.gameObject); // Destroy the power-up object

            if (scoreCounter != null) // Check if the ScoreCounter script is assigned
            {
                scoreCounter.IncreaseScore(20); // Increase the score by 20 using the ScoreCounter script
                AudioManager.PlaySfx(RetroSfx.Pickup);
            }
        }
        else if (other.gameObject.CompareTag("Obstacle")) // Check if the collided object is an obstacle
        {
            DestroyPlayer();
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

    // The only place where the player dies - used by trigger obstacles (above)
    // and by solid obstacles (CollisionHandler)
    public void DestroyPlayer()
    {
        if (isDead) return; // Both handlers can fire in the same frame
        isDead = true;
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        int finalScore = scoreCounter != null ? scoreCounter.Score : 0;
        AudioManager.PlaySfx(RetroSfx.Death);

        // Stop the cat where it got hit, so it stays visible during the hit-stop
        Movement movement = GetComponent<Movement>();
        if (movement != null) movement.enabled = false;
        if (scoreCounter != null) scoreCounter.CancelInvoke();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        CameraFollow cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cameraFollow != null) cameraFollow.Shake(shakeDuration, shakeStrength);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(finalScore);
        }
        else if (gameOverObject != null)
        {
            gameOverObject.SetActive(true); // No GameManager in the scene - just show the Game Over screen
        }

        Destroy(playerPrefab != null ? playerPrefab : gameObject); // Destroy the player object
    }
}
