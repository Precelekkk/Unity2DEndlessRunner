using UnityEngine;

public class ScoreSaveManager : MonoBehaviour
{
    public static ScoreSaveManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Reset the score to 0 when the game starts
        SaveScore(0);
    }

    public void SaveScore(int score)
    {
        PlayerPrefs.SetInt("PlayerScore", score);
    }

    public int LoadScore()
    {
        return PlayerPrefs.GetInt("PlayerScore", 0);
    }
}