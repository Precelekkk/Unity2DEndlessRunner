using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Applies the volume settings to every AudioSource in the game.
// Music = a long clip (the background tracks), everything else = sound effects.
// M mutes / unmutes. Creates itself when the game starts.
public class AudioManager : MonoBehaviour
{
    private const float MusicMinLength = 20f; // Clips longer than this many seconds count as music
    private const float RescanInterval = 0.5f; // Picks up sources spawned during the game

    // Volume each source had in the scene / prefab - settings multiply it, never overwrite it
    private readonly Dictionary<AudioSource, float> baseVolumes = new Dictionary<AudioSource, float>();
    private float rescanTimer;
    private AudioSource sfxSource;     // Plays the RetroSfx sounds (counts as an effect: short clips)

    public static AudioManager Instance { get; private set; }

    // One-shot sound effect, e.g. AudioManager.PlaySfx(RetroSfx.Death).
    // Keeps playing across scene changes, so a click that loads a scene is not cut off.
    public static void PlaySfx(AudioClip clip)
    {
        if (Instance != null && clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip);
        }
    }

    private void Awake()
    {
        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStart()
    {
        if (FindObjectOfType<AudioManager>() != null) return;

        GameObject go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioManager>();
    }

    private void OnEnable()
    {
        GameSettings.Changed += Apply;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply();
    }

    private void OnDisable()
    {
        GameSettings.Changed -= Apply;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameSettings.Muted = !GameSettings.Muted;
        }

        rescanTimer -= Time.unscaledDeltaTime;
        if (rescanTimer <= 0f)
        {
            Apply();
        }
    }

    private void Apply()
    {
        rescanTimer = RescanInterval;
        AudioListener.volume = GameSettings.Muted ? 0f : GameSettings.MasterVolume;

        foreach (AudioSource source in FindObjectsOfType<AudioSource>(true))
        {
            if (!baseVolumes.TryGetValue(source, out float baseVolume))
            {
                baseVolume = source.volume;
                baseVolumes[source] = baseVolume;
            }

            source.volume = baseVolume * (IsMusic(source) ? GameSettings.MusicVolume : GameSettings.SfxVolume);
        }

        // Forget sources from unloaded scenes / destroyed objects
        List<AudioSource> gone = null;
        foreach (AudioSource source in baseVolumes.Keys)
        {
            if (source == null) (gone ??= new List<AudioSource>()).Add(source);
        }
        if (gone != null)
        {
            foreach (AudioSource source in gone) baseVolumes.Remove(source);
        }
    }

    private static bool IsMusic(AudioSource source)
    {
        return source.clip != null && source.clip.length > MusicMinLength;
    }
}
