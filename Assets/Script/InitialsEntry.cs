using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// "NEW HIGH SCORE - enter your initials" screen, like on an arcade cabinet.
//   Up / Down     change the letter        Left / Right  move between letters
//   A-Z keys      type a letter directly    Backspace     go back
//   Enter or OK   save
// Opened by GameManager when a run gets into the top 10.
public class InitialsEntry : MonoBehaviour
{
    private const int Letters = 3;

    public static bool IsOpen => current != null;

    // True also in the frame the screen closed, so the same Enter / key press does not
    // also restart the game on the Game Over screen behind it
    public static bool BlocksKeys => IsOpen || Time.frameCount == closedFrame;

    private static InitialsEntry current;
    private static int closedFrame = -1;

    private readonly char[] letters = new char[Letters];
    private readonly Text[] letterTexts = new Text[Letters];
    private readonly Image[] slots = new Image[Letters];
    private int selected;
    private Action<string> onConfirm;

    public static void Open(int score, Action<string> onConfirm)
    {
        if (current != null) return;

        GameObject go = new GameObject("InitialsEntry");
        current = go.AddComponent<InitialsEntry>();
        current.onConfirm = onConfirm;
        current.Build(score, HighScoreTable.LastInitials);
    }

    private void Build(int score, string startInitials)
    {
        if (EventSystem.current == null)
        {
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
        }

        RetroUi.CreateCanvas(gameObject, 850, true); // Above the Game Over screen, below settings / CRT

        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(transform, false);
        RetroUi.Stretch((RectTransform)backdrop.transform);
        backdrop.GetComponent<Image>().color = RetroUi.Backdrop; // Also blocks the Game Over buttons

        Transform board = RetroUi.CreateBoard(transform, Vector2.zero, new Vector2(900f, 620f)).transform;
        RetroUi.CreateText(board, "NEW HIGH SCORE", 72, RetroUi.Highlight, new Vector2(0f, 220f), new Vector2(800f, 90f), TextAnchor.MiddleCenter);
        RetroUi.CreateText(board, "SCORE  " + score, 46, RetroUi.TextColor, new Vector2(0f, 140f), new Vector2(800f, 60f), TextAnchor.MiddleCenter);

        for (int i = 0; i < Letters; i++)
        {
            letters[i] = i < startInitials.Length ? startInitials[i] : 'A';

            GameObject slot = new GameObject("Slot " + i, typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(board, false);
            RectTransform rect = (RectTransform)slot.transform;
            rect.anchoredPosition = new Vector2((i - 1) * 150f, 0f);
            rect.sizeDelta = new Vector2(120f, 150f);
            slots[i] = slot.GetComponent<Image>();
            slots[i].raycastTarget = false;

            letterTexts[i] = RetroUi.CreateText(slot.transform, letters[i].ToString(), 120, RetroUi.Accent, Vector2.zero, rect.sizeDelta, TextAnchor.MiddleCenter);
        }

        RetroUi.CreateText(board, "UP DOWN  LETTER     LEFT RIGHT  MOVE", 30, RetroUi.LineColor, new Vector2(0f, -125f), new Vector2(820f, 50f), TextAnchor.MiddleCenter);
        RetroUi.CreateButton(board, "OK", new Vector2(0f, -220f), new Vector2(240f, 90f), 54, Confirm);

        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeLetter(1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeLetter(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Select(selected + 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Backspace)) Select(selected - 1);

        // Typing a letter puts it in the current slot and moves to the next one
        foreach (char c in Input.inputString)
        {
            char upper = char.ToUpperInvariant(c);
            if (upper >= 'A' && upper <= 'Z')
            {
                letters[selected] = upper;
                Select(selected + 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Confirm();
            return;
        }

        // The selected slot blinks, unscaled time like the rest of the menus
        bool on = Mathf.Repeat(Time.unscaledTime * 3f, 1f) < 0.6f;
        letterTexts[selected].color = on ? RetroUi.Highlight : RetroUi.Accent;
    }

    private void ChangeLetter(int step)
    {
        int index = (letters[selected] - 'A' + step + 26) % 26;
        letters[selected] = (char)('A' + index);
        AudioManager.PlaySfx(RetroSfx.Blip);
        Refresh();
    }

    private void Select(int index)
    {
        selected = Mathf.Clamp(index, 0, Letters - 1);
        AudioManager.PlaySfx(RetroSfx.Blip);
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < Letters; i++)
        {
            letterTexts[i].text = letters[i].ToString();
            letterTexts[i].color = RetroUi.Accent;
            slots[i].color = i == selected ? RetroUi.LineColor : RetroUi.Inset;
        }
    }

    private void Confirm()
    {
        if (current != this) return;

        current = null;
        closedFrame = Time.frameCount;
        AudioManager.PlaySfx(RetroSfx.Blip);
        onConfirm?.Invoke(new string(letters));
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (current == this) current = null; // Scene changed while the screen was open
    }
}
