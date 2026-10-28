using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The one place that knows the state of a run (playing / game over),
// shows the Game Over screen and keeps the best and last score.
public class GameManager : MonoBehaviour
{
    public enum State { Playing, GameOver }

    public const string BestScoreKey = "BestScore";
    public const string LastScoreKey = "LastScore";

    public static GameManager Instance { get; private set; }

    [Header("Game Over UI")]
    public GameObject gameOverScreen;
    public Text finalScoreText;
    public Text bestScoreText;

    [Header("Scenes")]
    public string menuScene = "Main Menu";

    public State CurrentState { get; private set; } = State.Playing;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (CurrentState != State.GameOver) return;
        if (InitialsEntry.BlocksKeys) return; // Typing the initials - R would restart the game

        // Keyboard shortcuts next to the Restart / Menu buttons
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !SettingsMenu.BlocksEscape)
        {
            BackToMenu();
        }
    }

    public void GameOver(int score)
    {
        if (CurrentState == State.GameOver) return;
        CurrentState = State.GameOver;

        bool newBest = RecordScore(score);

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "SCORE  " + score; // ArcadeClassic has no colon
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = newBest ? "NEW BEST SCORE!" : "BEST  " + PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        if (newBest)
        {
            StartCoroutine(PlayAfter(RetroSfx.NewRecord, 0.5f)); // After the death sound
        }

        // Top 10 run: ask for the initials on top of the Game Over screen
        if (HighScoreTable.Qualifies(score))
        {
            InitialsEntry.Open(score, initials =>
            {
                int rank = HighScoreTable.Add(initials, score);
                if (bestScoreText != null && rank > 1)
                {
                    bestScoreText.text = "TOP 10  PLACE " + rank;
                }
            });
        }
        else
        {
            HighScoreTable.ClearLastRank();
        }
    }

    private static IEnumerator PlayAfter(AudioClip clip, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        AudioManager.PlaySfx(clip);
    }

    // Saves the score of a finished run. Returns true if it beat the best score.
    public static bool RecordScore(int score)
    {
        bool newBest = score > PlayerPrefs.GetInt(BestScoreKey, 0);
        if (newBest)
        {
            PlayerPrefs.SetInt(BestScoreKey, score);
        }

        PlayerPrefs.SetInt(LastScoreKey, score);
        PlayerPrefs.Save();
        return newBest;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}
