using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("ShowText")]
    public Text scoreText; // Right Cor Txt showing a Score of the player ( Play 1 )
    public Text additionalScoreText; // Add a reference to the additional Text object

    [Header("GameObject From Game")]
    public GameObject powerUpPrefab;
    public GameObject popUpText;

    [Header("Counters")]
    public int MaxScoreFromLevel; // help me 
    private int score = 0;
    private int powerUpCounter = 0;

    [Header("BoolValues")]
    private bool isPlayerInScene = true;

    private void Start()
    {
        // Initialize the score from the ScoreSaveManager if it exists
        if (ScoreSaveManager.instance != null)
        {
            score = ScoreSaveManager.instance.LoadScore();
        }

        InvokeRepeating("UpdateScore", 1f, 1f); // Update the score every 1 second
        UpdateScoreText();
    }

    public void IncreaseScore(int value)
    {
        // Check if the player is not null before updating the score
        if (playerPrefab != null)
        {
            if (value == 20 && powerUpPrefab != null)
            {
                powerUpCounter++;
            }

            score += value;

            // Save the score when it changes
            if (ScoreSaveManager.instance != null)
            {
                ScoreSaveManager.instance.SaveScore(score);
            }

            UpdateScoreText();
            Debug.Log("Current score: " + score);

            if (value == 20 && powerUpPrefab != null)
            {
                popUpText.SetActive(true);
                Invoke("HidePopUpText", 1f);
            }
        }
    }

    public void ResetScoreOnPlayerDeath()
    {
        // Reset the score to 0 when the player is dead
        ResetScore();

        // You can add more logic here for handling player death
        Debug.Log("Player is dead. Score reset to 0.");
    }

    private void HidePopUpText()
    {
        if (popUpText != null)
        {
            popUpText.SetActive(false);
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
        additionalScoreText.text = "Score: " + score; // Update the additional score text
        // You can add more logic here to show additional stats when the player is dead
    }

    private void UpdateScore()
    {
        if (isPlayerInScene && playerPrefab != null)
        {
            score++;
            UpdateScoreText();
            Debug.Log("Current score: " + score);
        }
    }

    private void ResetScore()
    {
        // Reset the score to 0
        score = 0;

        // Save the reset score if the ScoreSaveManager exists
        if (ScoreSaveManager.instance != null)
        {
            ScoreSaveManager.instance.SaveScore(score);
        }

        UpdateScoreText();
        Debug.Log("Score reset to 0");
    }

    public void SaveScore()
    {
        if (ScoreSaveManager.instance != null)
        {
            ScoreSaveManager.instance.SaveScore(score);
            Debug.Log("Score saved: " + score);
        }
        else
        {
            Debug.LogWarning("ScoreSaveManager not found.");
        }
    }
}