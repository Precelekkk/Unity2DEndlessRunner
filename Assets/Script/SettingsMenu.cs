using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Settings window (volumes, mute, CRT effect, fullscreen), built in code so every
// scene can use it without extra setup. Opened from the SETTINGS button in the main
// menu and from the pause menu. Esc or BACK closes it.
// Looks like the rest of the game: parchment board, dark ink text, night-blue accents
// from the background and the ArcadeClassic pixel font (it has no % ( ) / : - signs).
public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance.window.activeSelf;

    // True also in the frame the window was closed, so the same Esc press does not
    // also resume the game / leave the Game Over screen
    public static bool BlocksEscape => IsOpen || (Instance != null && Time.frameCount == Instance.closedFrame);

    public string menuSceneName = "Main Menu";

    // Palette taken from the game art
    private static readonly Color Ink = new Color(0.17f, 0.14f, 0.13f);          // Outline of the parchment boards
    private static readonly Color Wood = new Color(0.55f, 0.37f, 0.24f);         // Dark edge of the parchment
    private static readonly Color Night = new Color(0.2f, 0.19f, 0.6f);          // Sky in the background
    private static readonly Color NightLight = new Color(0.33f, 0.32f, 0.82f);
    private static readonly Color Backdrop = new Color(0.05f, 0.04f, 0.16f, 0.78f);
    private static readonly Color Track = new Color(0.55f, 0.37f, 0.24f, 0.45f);

    private Font font;
    private Sprite parchment;
    private GameObject window;
    private RectTransform boardRect;
    private static readonly Vector2 BoardSize = new Vector2(980f, 900f);
    private GameObject menuButton;
    private int closedFrame = -1;

    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Toggle muteToggle;
    private Toggle crtToggle;
    private Toggle fullscreenToggle;
    private bool refreshing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStart()
    {
        if (FindObjectOfType<SettingsMenu>() != null) return;

        GameObject go = new GameObject("SettingsMenu");
        DontDestroyOnLoad(go);
        go.AddComponent<SettingsMenu>();
    }

    public static void Open()
    {
        if (Instance == null) return;
        Instance.EnsureEventSystem();
        Instance.Refresh();
        Instance.window.SetActive(true);
        Instance.FitToScreen();
    }

    public static void Close()
    {
        if (Instance == null || !Instance.window.activeSelf) return;
        Instance.window.SetActive(false);
        Instance.closedFrame = Time.frameCount;
    }

    private void Awake()
    {
        Instance = this;
        font = Resources.Load<Font>("Fonts/ARCADECLASSIC");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        parchment = Resources.Load<Sprite>("UI/Parchment");

        BuildUi();
        window.SetActive(false);
    }

    private void OnEnable()
    {
        GameSettings.Changed += Refresh;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        GameSettings.Changed -= Refresh;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Close();
        menuButton.SetActive(scene.name == menuSceneName); // The pause menu has its own button
    }

    private void Update()
    {
        if (IsOpen)
        {
            FitToScreen();
        }

        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    // Shrinks the board when the screen is too small for it (very wide or very small
    // windows, a browser tab) - it never grows above its normal size
    private void FitToScreen()
    {
        Rect screen = ((RectTransform)transform).rect;
        float scale = Mathf.Min(1f, screen.width * 0.94f / BoardSize.x, screen.height * 0.94f / BoardSize.y);
        boardRect.localScale = new Vector3(scale, scale, 1f);
    }

    // Puts the current settings into the controls (also when M / F2 change them)
    private void Refresh()
    {
        refreshing = true;
        masterSlider.value = GameSettings.MasterVolume;
        musicSlider.value = GameSettings.MusicVolume;
        sfxSlider.value = GameSettings.SfxVolume;
        muteToggle.isOn = GameSettings.Muted;
        crtToggle.isOn = GameSettings.CrtEnabled;
        fullscreenToggle.isOn = GameSettings.Fullscreen;
        refreshing = false;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    // ---------------- UI building ----------------

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900; // Above the game UI, below the CRT overlay

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // SETTINGS board in the bottom right corner of the main menu
        menuButton = CreateButton(transform, "SETTINGS", new Vector2(-200f, 80f), new Vector2(340f, 110f), 54, Open);
        RectTransform menuRect = menuButton.GetComponent<RectTransform>();
        menuRect.anchorMin = menuRect.anchorMax = new Vector2(1f, 0f);

        // Night-blue backdrop over the whole screen - also blocks clicks on the game UI behind it
        window = new GameObject("Settings Window", typeof(RectTransform), typeof(Image));
        window.transform.SetParent(transform, false);
        Stretch(window.GetComponent<RectTransform>());
        window.GetComponent<Image>().color = Backdrop;

        GameObject board = CreateBoard(window.transform, Vector2.zero, BoardSize);
        boardRect = board.GetComponent<RectTransform>();
        Transform panel = board.transform;

        CreateText(panel, "SETTINGS", 96, Ink, new Vector2(0f, 345f), new Vector2(800f, 100f), TextAnchor.MiddleCenter);

        CreateSection(panel, "AUDIO", 255f);
        masterSlider = CreateSliderRow(panel, "MASTER", 185f, v => GameSettings.MasterVolume = v);
        musicSlider = CreateSliderRow(panel, "MUSIC", 120f, v => GameSettings.MusicVolume = v);
        sfxSlider = CreateSliderRow(panel, "EFFECTS", 55f, v => GameSettings.SfxVolume = v);
        muteToggle = CreateToggleRow(panel, "MUTE", "M", -10f, v => GameSettings.Muted = v);

        CreateSection(panel, "DISPLAY", -95f);
        crtToggle = CreateToggleRow(panel, "RETRO SCREEN", "F2", -165f, v => GameSettings.CrtEnabled = v);
        fullscreenToggle = CreateToggleRow(panel, "FULLSCREEN", "", -230f, v => GameSettings.Fullscreen = v);

        CreateButton(panel, "BACK", new Vector2(0f, -340f), new Vector2(300f, 100f), 56, Close);
    }

    // Parchment board like the menu buttons (9-sliced, so it can be any size)
    private GameObject CreateBoard(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("Board", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        if (parchment != null)
        {
            image.sprite = parchment;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = size.y < 200f ? 2.2f : 1f; // Thinner torn edges on small buttons
            image.color = Color.white;
        }
        else
        {
            image.color = new Color(0.86f, 0.73f, 0.55f); // Plain parchment colour if the sprite is missing
        }
        return go;
    }

    // Section title with a thin wooden line under it
    private void CreateSection(Transform parent, string title, float y)
    {
        CreateText(parent, title, 50, Night, new Vector2(-360f, y), new Vector2(400f, 60f), TextAnchor.MiddleLeft);

        GameObject line = new GameObject(title + " Line", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(parent, false);
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, y - 32f);
        rect.sizeDelta = new Vector2(780f, 4f);
        Image image = line.GetComponent<Image>();
        image.color = Wood;
        image.raycastTarget = false;
    }

    private Slider CreateSliderRow(Transform parent, string label, float y, Action<float> onChange)
    {
        CreateText(parent, label, 44, Ink, new Vector2(-250f, y), new Vector2(260f, 60f), TextAnchor.MiddleLeft);
        Text valueText = CreateText(parent, "0", 44, Night, new Vector2(345f, y), new Vector2(100f, 60f), TextAnchor.MiddleRight);

        GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(90f, y);
        rect.sizeDelta = new Vector2(360f, 18f);

        go.transform.Find("Background").GetComponent<Image>().color = Track;
        go.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Night;

        // Square "pixel" handle with a dark ink frame
        Image handle = go.transform.Find("Handle Slide Area/Handle").GetComponent<Image>();
        handle.color = Ink;
        handle.rectTransform.sizeDelta = new Vector2(30f, 18f);
        GameObject handleInside = new GameObject("Inside", typeof(RectTransform), typeof(Image));
        handleInside.transform.SetParent(handle.transform, false);
        RectTransform insideRect = handleInside.GetComponent<RectTransform>();
        Stretch(insideRect);
        insideRect.offsetMin = new Vector2(5f, 5f);
        insideRect.offsetMax = new Vector2(-5f, -5f);
        Image inside = handleInside.GetComponent<Image>();
        inside.color = NightLight;
        inside.raycastTarget = false;

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        ColorBlock colors = slider.colors;
        colors.highlightedColor = new Color(0.85f, 0.85f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.9f);
        slider.colors = colors;
        slider.onValueChanged.AddListener(v =>
        {
            valueText.text = Mathf.RoundToInt(v * 100f).ToString(); // The font has no % sign
            if (!refreshing) onChange(v);
        });
        return slider;
    }

    private Toggle CreateToggleRow(Transform parent, string label, string key, float y, Action<bool> onChange)
    {
        GameObject go = DefaultControls.CreateToggle(new DefaultControls.Resources());
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-50f, y);
        rect.sizeDelta = new Vector2(600f, 54f);

        // Box: dark ink frame with a parchment-coloured inside
        Image box = go.transform.Find("Background").GetComponent<Image>();
        RectTransform boxRect = box.rectTransform;
        boxRect.anchorMin = boxRect.anchorMax = new Vector2(0f, 0.5f);
        boxRect.pivot = new Vector2(0f, 0.5f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(46f, 46f);
        box.color = Ink;

        Image check = go.transform.Find("Background/Checkmark").GetComponent<Image>();
        check.color = Night;
        Stretch(check.rectTransform);
        check.rectTransform.offsetMin = new Vector2(10f, 10f);
        check.rectTransform.offsetMax = new Vector2(-10f, -10f);

        GameObject boxInside = new GameObject("Inside", typeof(RectTransform), typeof(Image));
        boxInside.transform.SetParent(box.transform, false);
        boxInside.transform.SetSiblingIndex(0); // Behind the checkmark
        RectTransform insideRect = boxInside.GetComponent<RectTransform>();
        Stretch(insideRect);
        insideRect.offsetMin = new Vector2(5f, 5f);
        insideRect.offsetMax = new Vector2(-5f, -5f);
        Image inside = boxInside.GetComponent<Image>();
        inside.color = new Color(0.93f, 0.84f, 0.68f);
        inside.raycastTarget = false;

        Text text = go.transform.Find("Label").GetComponent<Text>();
        StyleText(text, label, 44, Ink, TextAnchor.MiddleLeft);
        text.rectTransform.offsetMin = new Vector2(70f, 0f);

        if (!string.IsNullOrEmpty(key))
        {
            // Keyboard shortcut on the right, in the wood colour
            CreateText(parent, key, 40, Wood, new Vector2(345f, y), new Vector2(100f, 60f), TextAnchor.MiddleRight);
        }

        Toggle toggle = go.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(v =>
        {
            if (!refreshing) onChange(v);
        });
        return toggle;
    }

    private GameObject CreateButton(Transform parent, string label, Vector2 position, Vector2 size, int fontSize, Action onClick)
    {
        GameObject go = CreateBoard(parent, position, size);
        go.name = label + " Button";

        Button button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.82f, 0.82f, 1f);   // Slightly blue on hover
        colors.pressedColor = new Color(0.62f, 0.6f, 0.85f);
        button.colors = colors;
        button.onClick.AddListener(() => onClick());

        Text text = CreateText(go.transform, label, fontSize, Ink, Vector2.zero, size, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        return go;
    }

    private Text CreateText(Transform parent, string content, int size, Color color, Vector2 position, Vector2 boxSize, TextAnchor alignment)
    {
        GameObject go = new GameObject(content, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;

        Text text = go.GetComponent<Text>();
        StyleText(text, content, size, color, alignment);
        return text;
    }

    private void StyleText(Text text, string content, int size, Color color, TextAnchor alignment)
    {
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyle.Normal; // Bold looks blurry on a pixel font
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
