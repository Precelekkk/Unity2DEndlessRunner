using UnityEngine;
using UnityEngine.SceneManagement;

// End-of-level trigger: saves the score and loads the next scene.
public class GameSceneChange : MonoBehaviour
{
    public string targetScene = "MainMenuEnd";
    public string playerTag = "Player"; // Tag to identify the player

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (!other.CompareTag(playerTag)) return;

        // Save the current score before changing scenes
        ScoreCounter scoreCounter = FindObjectOfType<ScoreCounter>();
        if (scoreCounter != null)
        {
            scoreCounter.SaveScore();
        }
        else
        {
            Debug.LogWarning("ScoreCounter not found in the scene.");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene);
    }
}
