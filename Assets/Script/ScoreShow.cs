using UnityEngine;
using UnityEngine.UI;

public class ScoreShow : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject powerUpPrefab;
    public GameObject popUpText;
    public Text scoreText; // Assign the UI Text object to this variable in the Unity editor
    public Text scoreTextEnd;
    

    private int score = 0;
    private int powerUpCounter = 0;
    private bool isPlayerInScene = true;

    private void Start()
    {
        InvokeRepeating("UpdateScore", 1f, 1f); // Update the score every 1 second
        UpdateScoreText();
    }

    public void IncreaseScore(int value)
    {
        if (value == 20 && powerUpPrefab != null)
        {
            powerUpCounter++;
        }

        score += value;
        UpdateScoreText();

        if (value == 20 && powerUpPrefab != null)
        {
            if (popUpText != null)
            {
                popUpText.SetActive(true);
                Invoke("HidePopUpText", 5f);
            }
        }
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
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score + " (" + powerUpCounter + ")";
            scoreTextEnd.text = "Score: " + score + " (" + powerUpCounter + ")";
            
        }
    }

    private void UpdateScore()
    {
        isPlayerInScene = playerPrefab != null; // Check if the player is in the scene

        if (isPlayerInScene)
        {
            score++;
            UpdateScoreText();
        }
        else
        {
            if (popUpText != null)
            {
                popUpText.SetActive(true);
                Invoke("HidePopUpText", 5f);
            }
        }
    }
}