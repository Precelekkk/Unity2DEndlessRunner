using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Old arcade monitor (CRT) look on top of every scene:
//  - dark horizontal scanlines
//  - a soft bright bar rolling from the top to the bottom of the screen
//  - slight flicker and dark rounded corners
//  - chromatic aberration + film grain through a URP volume
// It creates itself when the game starts, so no scene needs to be set up.
// F2 or the Settings menu turns it on / off (GameSettings.CrtEnabled).
public class RetroScreen : MonoBehaviour
{
    [Header("Scanlines")]
    public int linePixels = 2;            // Height of one dark line (and of the gap) in screen pixels
    public float lineDarkness = 0.28f;
    public float flicker = 0.04f;         // How much the lines pulse

    [Header("Rolling bar")]
    public float barHeight = 0.18f;       // Part of the screen height
    public float barBrightness = 0.07f;
    public float barSeconds = 7f;         // Time to roll over the whole screen

    [Header("Corners")]
    public float cornerDarkness = 0.55f;

    [Header("Post-processing")]
    public float chromaticAberration = 0.35f;
    public float filmGrain = 0.25f;

    private GameObject overlay;
    private RawImage scanlines;
    private RectTransform bar;
    private Volume volume;
    private int lastScreenHeight;
    private bool isOn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStart()
    {
        if (FindObjectOfType<RetroScreen>() != null) return;

        GameObject go = new GameObject("RetroScreen");
        DontDestroyOnLoad(go);
        go.AddComponent<RetroScreen>();
    }

    private void Awake()
    {
        BuildOverlay();
        BuildVolume();
        ApplySetting();
    }

    private void OnEnable()
    {
        GameSettings.Changed += ApplySetting;
    }

    private void OnDisable()
    {
        GameSettings.Changed -= ApplySetting;
    }

    private void ApplySetting()
    {
        if (overlay != null) SetOn(GameSettings.CrtEnabled);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            GameSettings.CrtEnabled = !GameSettings.CrtEnabled; // Calls ApplySetting through GameSettings.Changed
        }

        if (!isOn) return;

        if (Screen.height != lastScreenHeight)
        {
            UpdateScanlineTiling();
        }

        // Unscaled time - the effect keeps running in the pause menu and during the hit-stop
        float t = Time.unscaledTime;
        float pulse = 1f + Mathf.Sin(t * 60f) * flicker + Mathf.Sin(t * 7.3f) * flicker * 0.5f;
        scanlines.color = new Color(0f, 0f, 0f, lineDarkness * pulse);

        float progress = (t % barSeconds) / barSeconds;          // 0 -> 1
        float y = Mathf.Lerp(0.5f + barHeight, -0.5f - barHeight, progress);
        bar.anchoredPosition = new Vector2(0f, y * Screen.height);
    }

    private void SetOn(bool on)
    {
        isOn = on;
        overlay.SetActive(on);
        volume.enabled = on;
    }

    private void BuildOverlay()
    {
        overlay = new GameObject("CRT Overlay");
        overlay.transform.SetParent(transform, false);

        Canvas canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Above the game UI as well - a real CRT covers everything
        // No GraphicRaycaster: the overlay must never block button clicks

        CreateLayer("Corners", MakeCornerTexture(128), new Color(1f, 1f, 1f, cornerDarkness));

        bar = CreateLayer("Rolling Bar", MakeBarTexture(64), new Color(1f, 1f, 1f, barBrightness)).rectTransform;
        bar.anchorMin = new Vector2(0f, 0.5f);
        bar.anchorMax = new Vector2(1f, 0.5f);
        bar.sizeDelta = new Vector2(0f, Screen.height * barHeight);

        scanlines = CreateLayer("Scanlines", MakeScanlineTexture(), new Color(0f, 0f, 0f, lineDarkness));
        UpdateScanlineTiling();
    }

    private RawImage CreateLayer(string name, Texture2D texture, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(overlay.transform, false);

        RawImage image = go.AddComponent<RawImage>();
        image.texture = texture;
        image.color = color;
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private void UpdateScanlineTiling()
    {
        lastScreenHeight = Screen.height;
        float repeats = Screen.height / (2f * Mathf.Max(1, linePixels)); // One repeat = one dark line + one gap
        scanlines.uvRect = new Rect(0f, 0f, 1f, repeats);

        if (bar != null)
        {
            bar.sizeDelta = new Vector2(0f, Screen.height * barHeight);
        }
    }

    // 1x2 texture: one transparent pixel, one white pixel (tinted black by the RawImage color)
    private static Texture2D MakeScanlineTexture()
    {
        Texture2D texture = new Texture2D(1, 2, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
        texture.SetPixel(0, 1, Color.white);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        return texture;
    }

    // Soft vertical glow: transparent at the edges, white in the middle
    private static Texture2D MakeBarTexture(int height)
    {
        Texture2D texture = new Texture2D(1, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            float t = y / (height - 1f);
            float alpha = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0
            texture.SetPixel(0, y, new Color(1f, 1f, 1f, alpha * alpha));
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    // Dark rounded corners like the edge of a curved CRT glass
    private static Texture2D MakeCornerTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distance to the edge in a "rounded rectangle" way
                float dx = Mathf.Abs(x / (size - 1f) * 2f - 1f);
                float dy = Mathf.Abs(y / (size - 1f) * 2f - 1f);
                float d = Mathf.Pow(Mathf.Pow(dx, 6f) + Mathf.Pow(dy, 6f), 1f / 6f);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.82f, 1.02f, d));
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    private void BuildVolume()
    {
        // A runtime-only global volume: it adds to the volumes already in the scenes
        // and does not change any profile asset in the project
        volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

        ChromaticAberration aberration = profile.Add<ChromaticAberration>(true);
        aberration.intensity.Override(chromaticAberration);

        FilmGrain grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Thin2);
        grain.intensity.Override(filmGrain);

        volume.sharedProfile = profile;
    }
}
