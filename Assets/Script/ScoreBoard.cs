using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Score scene: the arcade top 10 (place, initials, score) on a blue board,
// the row of the last run highlighted, and the score of the last game below.
// The two old texts of the scene are hidden - the table replaces them.
public class ScoreBoard : MonoBehaviour
{
    public Text highScoreText;
    public Text lastScoreText;

    private const float RowHeight = 46f;

    private void Start()
    {
        if (highScoreText != null) highScoreText.gameObject.SetActive(false);
        if (lastScoreText != null) lastScoreText.gameObject.SetActive(false);

        GameObject host = new GameObject("Top 10");
        RetroUi.CreateCanvas(host, 10, false); // Above the scene UI; not clickable, so BACK still works

        Transform board = RetroUi.CreateBoard(host.transform, new Vector2(0f, -165f), new Vector2(720f, 680f)).transform;
        RetroUi.CreateText(board, "TOP 10", 64, RetroUi.Highlight, new Vector2(0f, 275f), new Vector2(600f, 80f), TextAnchor.MiddleCenter);

        List<HighScoreTable.Entry> entries = HighScoreTable.Load();
        int lastRank = HighScoreTable.LastRank;

        for (int i = 0; i < HighScoreTable.Size; i++)
        {
            float y = 205f - i * RowHeight;
            bool filled = i < entries.Count;
            Color color = !filled ? RetroUi.LineColor : (i + 1 == lastRank ? RetroUi.Highlight : RetroUi.TextColor);

            RetroUi.CreateText(board, (i + 1).ToString(), 40, color, new Vector2(-230f, y), new Vector2(100f, 50f), TextAnchor.MiddleRight);
            RetroUi.CreateText(board, filled ? entries[i].Initials : "", 40, color, new Vector2(-50f, y), new Vector2(160f, 50f), TextAnchor.MiddleCenter);
            RetroUi.CreateText(board, filled ? entries[i].Score.ToString() : "", 40, color, new Vector2(200f, y), new Vector2(200f, 50f), TextAnchor.MiddleRight);
        }

        RetroUi.CreateText(board, "LAST GAME  " + PlayerPrefs.GetInt(GameManager.LastScoreKey, 0), 40, RetroUi.Accent,
            new Vector2(0f, -280f), new Vector2(600f, 60f), TextAnchor.MiddleCenter);
    }
}
