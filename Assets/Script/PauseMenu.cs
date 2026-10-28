using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public float buttonSpacing = 93f; // Distance between the buttons of the pause panel

    private bool isPaused = false;

    void Start()
    {
        AddPauseButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !SettingsMenu.BlocksEscape)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        SettingsMenu.Close();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    void OnDestroy()
    {
        // PauseMenu lives on the Player - if the cat dies while paused, unfreeze the game
        Time.timeScale = 1f;
    }

    // The pause panel in the scene only has "Main Menu" and "Restart".
    // Adds RESUME on top and SETTINGS at the bottom, styled like the existing buttons.
    void AddPauseButtons()
    {
        if (pauseMenuUI == null) return;

        Button[] buttons = pauseMenuUI.GetComponentsInChildren<Button>(true);
        if (buttons.Length == 0) return;

        float topY = float.MinValue;
        float bottomY = float.MaxValue;
        foreach (Button button in buttons)
        {
            RectTransform rect = (RectTransform)button.transform;
            topY = Mathf.Max(topY, rect.anchoredPosition.y);
            rect.anchoredPosition -= new Vector2(0f, buttonSpacing); // Make room for RESUME
            bottomY = Mathf.Min(bottomY, rect.anchoredPosition.y);
        }

        CloneButton(buttons[0], "Resume", topY, Resume);
        CloneButton(buttons[0], "Settings", bottomY - buttonSpacing, SettingsMenu.Open);
    }

    void CloneButton(Button template, string label, float y, UnityEngine.Events.UnityAction onClick)
    {
        GameObject clone = Instantiate(template.gameObject, template.transform.parent);
        clone.name = label;

        // Drop the template's own scripts (e.g. BackToMenu) - only the UI components stay
        foreach (MonoBehaviour behaviour in clone.GetComponents<MonoBehaviour>())
        {
            if (!(behaviour is Graphic) && !(behaviour is Selectable))
            {
                Destroy(behaviour);
            }
        }

        Button button = clone.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent(); // Also removes the click events set in the editor
        button.onClick.AddListener(onClick);

        RectTransform rect = (RectTransform)clone.transform;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);

        Text text = clone.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }
}
