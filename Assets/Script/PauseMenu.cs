using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public float buttonSpacing = 70f; // Distance between the buttons of the pause panel

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
    // Adds RESUME on top and SETTINGS at the bottom, styled like the existing buttons,
    // spaces all four evenly and dims the game behind the panel.
    void AddPauseButtons()
    {
        if (pauseMenuUI == null) return;

        Button[] sceneButtons = pauseMenuUI.GetComponentsInChildren<Button>(true);
        if (sceneButtons.Length == 0) return;

        List<Button> buttons = new List<Button>(sceneButtons);
        buttons.Sort((a, b) => ((RectTransform)b.transform).anchoredPosition.y.CompareTo(((RectTransform)a.transform).anchoredPosition.y));
        float topY = ((RectTransform)buttons[0].transform).anchoredPosition.y;

        buttons.Insert(0, CloneButton(sceneButtons[0], "RESUME", Resume));
        buttons.Add(CloneButton(sceneButtons[0], "SETTINGS", SettingsMenu.Open));

        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform rect = (RectTransform)buttons[i].transform;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, topY - i * buttonSpacing);
        }

        AddBackdrop();
    }

    // Big dark rectangle behind the pause panel (the panel itself is small)
    void AddBackdrop()
    {
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(pauseMenuUI.transform, false);
        backdrop.transform.SetAsFirstSibling();
        ((RectTransform)backdrop.transform).sizeDelta = new Vector2(5000f, 5000f);
        backdrop.GetComponent<Image>().color = RetroUi.Backdrop;
    }

    Button CloneButton(Button template, string label, UnityEngine.Events.UnityAction onClick)
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

        Text text = clone.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
        return button;
    }
}
