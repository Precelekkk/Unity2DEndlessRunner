using UnityEngine;
using UnityEngine.SceneManagement;

// Arcade title screen extra on the main menu:
//  - blinking "PRESS SPACE TO START" at the bottom (Space / Enter starts a run)
// Creates itself in the main menu scene.
public class PressStart : MonoBehaviour
{
    private const string MenuScene = "Main Menu";
    private const string GameScene = "PlaySec";

    public float blinkSeconds = 1.1f;

    private CanvasGroup pressGroup;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) => CreateIfMenu(scene);
        CreateIfMenu(SceneManager.GetActiveScene());
    }

    private static void CreateIfMenu(Scene scene)
    {
        if (scene.name != MenuScene || FindObjectOfType<PressStart>() != null) return;
        new GameObject("PressStart").AddComponent<PressStart>();
    }

    private void Awake()
    {
        RetroUi.CreateCanvas(gameObject, 800, false); // Under the settings window and the CRT overlay

        GameObject press = new GameObject("Press Start", typeof(RectTransform), typeof(CanvasGroup));
        press.transform.SetParent(transform, false);
        RectTransform pressRect = (RectTransform)press.transform;
        pressRect.anchorMin = pressRect.anchorMax = new Vector2(0.5f, 0f);
        pressRect.anchoredPosition = new Vector2(0f, 55f);
        pressGroup = press.GetComponent<CanvasGroup>();
        // Three spaces between the words - the pixel font's single space is very narrow
        RetroUi.CreateText(press.transform, "PRESS   SPACE   TO   START", 54, RetroUi.Highlight, Vector2.zero, new Vector2(1100f, 70f), TextAnchor.MiddleCenter);
    }

    private void Update()
    {
        // Visible 70 % of the time, like an arcade attract screen
        pressGroup.alpha = Mathf.Repeat(Time.unscaledTime / blinkSeconds, 1f) < 0.7f ? 1f : 0f;

        if (SettingsMenu.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AudioManager.PlaySfx(RetroSfx.Blip);
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameScene);
        }
    }
}
