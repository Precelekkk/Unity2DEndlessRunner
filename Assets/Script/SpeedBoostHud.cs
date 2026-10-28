using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shows how much of the speed power-up is left: a small parchment board in the
// top left corner with "SPEED", a draining bar and the seconds left. It blinks
// for the last seconds so the end of the boost does not come as a surprise.
// Creates itself in every scene that has a player.
public class SpeedBoostHud : MonoBehaviour
{
    public float warnSeconds = 2f;   // Blink when this little is left
    public float blinkSpeed = 10f;

    private static readonly Vector2 BarSize = new Vector2(200f, 22f);

    private Movement movement;
    private CanvasGroup group;
    private RectTransform fill;
    private Text secondsText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) => CreateIfRunScene();
        CreateIfRunScene();
    }

    private static void CreateIfRunScene()
    {
        if (FindObjectOfType<Movement>() == null) return; // Only in the game scene
        if (FindObjectOfType<SpeedBoostHud>() != null) return;

        new GameObject("SpeedBoostHud").AddComponent<SpeedBoostHud>();
    }

    private void Awake()
    {
        movement = FindObjectOfType<Movement>();

        RetroUi.CreateCanvas(gameObject, 800, false); // Under the settings window and the CRT overlay
        group = gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;

        GameObject board = RetroUi.CreateBoard(transform, new Vector2(280f, -85f), new Vector2(500f, 120f));
        RectTransform boardRect = board.GetComponent<RectTransform>();
        boardRect.anchorMin = boardRect.anchorMax = new Vector2(0f, 1f);
        board.GetComponent<Image>().raycastTarget = false;

        RetroUi.CreateText(board.transform, "SPEED", 44, RetroUi.Accent, new Vector2(-150f, 2f), new Vector2(170f, 60f), TextAnchor.MiddleCenter);

        // Bar: dark blue track with a lavender fill that shrinks from the right
        Image track = CreateImage(board.transform, "Track", RetroUi.Inset);
        track.rectTransform.anchoredPosition = new Vector2(45f, 0f);
        track.rectTransform.sizeDelta = BarSize;

        Image fillImage = CreateImage(track.transform, "Fill", RetroUi.Accent);
        fill = fillImage.rectTransform;
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(1f, 1f);
        fill.pivot = new Vector2(0f, 0.5f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        secondsText = RetroUi.CreateText(board.transform, "10", 44, RetroUi.TextColor, new Vector2(195f, 2f), new Vector2(60f, 60f), TextAnchor.MiddleCenter);
    }

    private void Update()
    {
        if (movement == null || !movement.IsBoosted || movement.BoostDuration <= 0f)
        {
            group.alpha = 0f;
            if (movement == null) Destroy(gameObject); // The cat died
            return;
        }

        float timeLeft = movement.BoostTimeLeft;
        fill.anchorMax = new Vector2(Mathf.Clamp01(timeLeft / movement.BoostDuration), 1f);
        secondsText.text = Mathf.CeilToInt(timeLeft).ToString();

        // Blink at the end; unscaled time so it keeps blinking in the pause menu
        bool warn = timeLeft <= warnSeconds;
        group.alpha = warn && Mathf.Sin(Time.unscaledTime * blinkSpeed) < 0f ? 0.35f : 1f;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
}
