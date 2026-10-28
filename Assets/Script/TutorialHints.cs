using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Short "how to play" board at the top of the screen at the start of every run.
// The first obstacles come after SpawnManager.startDistance, so there is time to read it.
// Creates itself in every scene that has a player.
public class TutorialHints : MonoBehaviour
{
    public static bool Enabled = true;    // Switch off while testing obstacles

    public float showSeconds = 5f;
    public float fadeSeconds = 0.8f;

    private static readonly string[][] Lines =
    {
        new[] { "SPACE", "JUMP" },
        new[] { "SPACE IN THE AIR", "SECOND JUMP" },
        new[] { "SPACE AGAIN", "SLAM DOWN" },
    };

    private CanvasGroup group;
    private PlayerController player;
    private float timer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) => CreateIfRunScene();
        CreateIfRunScene();
    }

    private static void CreateIfRunScene()
    {
        if (!Enabled) return;
        if (FindObjectOfType<PlayerController>() == null) return; // Only in the game scene
        if (FindObjectOfType<TutorialHints>() != null) return;

        new GameObject("TutorialHints").AddComponent<TutorialHints>();
    }

    private void Awake()
    {
        RetroUi.CreateCanvas(gameObject, 800, false); // Under the settings window and the CRT overlay
        group = gameObject.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        GameObject board = RetroUi.CreateBoard(transform, new Vector2(0f, -190f), new Vector2(900f, 290f));
        RectTransform boardRect = board.GetComponent<RectTransform>();
        boardRect.anchorMin = boardRect.anchorMax = new Vector2(0.5f, 1f);
        board.GetComponent<Image>().raycastTarget = false;

        // Two columns that meet in the middle: key on the left, action on the right
        for (int i = 0; i < Lines.Length; i++)
        {
            float y = 75f - i * 75f;
            Text key = RetroUi.CreateText(board.transform, Lines[i][0], 46, RetroUi.Accent, new Vector2(-25f, y), new Vector2(400f, 60f), TextAnchor.MiddleRight);
            key.rectTransform.pivot = new Vector2(1f, 0.5f);
            Text action = RetroUi.CreateText(board.transform, Lines[i][1], 46, RetroUi.TextColor, new Vector2(25f, y), new Vector2(400f, 60f), TextAnchor.MiddleLeft);
            action.rectTransform.pivot = new Vector2(0f, 0.5f);
        }

        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // Scaled time: the hints wait while the game is paused
        timer += Time.deltaTime;
        group.alpha = 1f - Mathf.Clamp01((timer - showSeconds) / fadeSeconds);

        if (timer >= showSeconds + fadeSeconds || player == null)
        {
            Destroy(gameObject); // Faded out, or the cat died during the tutorial
        }
    }
}
