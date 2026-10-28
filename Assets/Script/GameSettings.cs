using System;
using UnityEngine;

// Player options saved in PlayerPrefs: volumes, mute, CRT effect, fullscreen.
// Anything that depends on them listens to the Changed event.
public static class GameSettings
{
    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";
    private const string MutedKey = "Muted";
    private const string CrtKey = "RetroCRT";

    public static event Action Changed;

    private static bool loaded;
    private static float master;
    private static float music;
    private static float sfx;
    private static bool muted;
    private static bool crt;

    public static float MasterVolume
    {
        get { Load(); return master; }
        set { Load(); master = Mathf.Clamp01(value); Save(MasterKey, master); }
    }

    public static float MusicVolume
    {
        get { Load(); return music; }
        set { Load(); music = Mathf.Clamp01(value); Save(MusicKey, music); }
    }

    public static float SfxVolume
    {
        get { Load(); return sfx; }
        set { Load(); sfx = Mathf.Clamp01(value); Save(SfxKey, sfx); }
    }

    public static bool Muted
    {
        get { Load(); return muted; }
        set { Load(); muted = value; Save(MutedKey, value ? 1f : 0f); }
    }

    public static bool CrtEnabled
    {
        get { Load(); return crt; }
        set { Load(); crt = value; Save(CrtKey, value ? 1f : 0f); }
    }

    // Unity remembers the window mode itself, so this is not stored here
    public static bool Fullscreen
    {
        get { return Screen.fullScreen; }
        set { Screen.fullScreen = value; Changed?.Invoke(); }
    }

    private static void Load()
    {
        if (loaded) return;
        loaded = true;

        master = PlayerPrefs.GetFloat(MasterKey, 1f);
        music = PlayerPrefs.GetFloat(MusicKey, 0.8f);
        sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
        muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        crt = PlayerPrefs.GetInt(CrtKey, 1) == 1;
    }

    private static void Save(string key, float value)
    {
        if (key == MutedKey || key == CrtKey)
        {
            PlayerPrefs.SetInt(key, value > 0.5f ? 1 : 0);
        }
        else
        {
            PlayerPrefs.SetFloat(key, value);
        }

        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
