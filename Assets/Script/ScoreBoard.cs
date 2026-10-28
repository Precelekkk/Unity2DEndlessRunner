using UnityEngine;
using UnityEngine.UI;

// Fills the texts of the Score scene with the saved best and last score.
public class ScoreBoard : MonoBehaviour
{
    public Text highScoreText;
    public Text lastScoreText;

    private void Start()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
        }

        if (lastScoreText != null)
        {
            lastScoreText.text = "Last Game Score: " + PlayerPrefs.GetInt(GameManager.LastScoreKey, 0);
        }
    }
}
