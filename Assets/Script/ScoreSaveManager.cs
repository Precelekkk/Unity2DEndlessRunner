using UnityEngine;

// Keeps the score between scenes in PlayerPrefs.
// It lives on the Player, so it must NOT use DontDestroyOnLoad - that used to
// carry the cat into the menu and destroy the new cat on the next run.
public class ScoreSaveManager : MonoBehaviour
{
    public static ScoreSaveManager instance;

    void Awake()
    {
        instance = this;

        // Reset the score to 0 when a run starts
        SaveScore(0);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
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
