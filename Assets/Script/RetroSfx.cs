using System.Collections.Generic;
using UnityEngine;

// 8-bit style sound effects made in code (square / triangle waves and noise),
// like the sound chips of old arcade machines. No audio files needed.
// Play them with AudioManager.PlaySfx(RetroSfx.Death) etc.
public static class RetroSfx
{
    private const int Rate = 44100;

    private enum Wave { Square, Triangle, Noise }

    private static AudioClip death, powerUp, pickup, slam, newRecord, blip;
    private static readonly System.Random random = new System.Random(7); // Same noise every time

    // Falling "game over" tone + crash
    public static AudioClip Death => death != null ? death : death = Make("Death",
        Tone(523f, 98f, 0.42f, Wave.Square, 0.35f),
        Tone(0f, 0f, 0.18f, Wave.Noise, 0.3f));

    // Rising arpeggio for the speed power-up
    public static AudioClip PowerUp => powerUp != null ? powerUp : powerUp = Make("PowerUp",
        Notes(0.06f, Wave.Square, 0.28f, 392f, 523f, 659f, 784f, 1047f));

    // Short coin "blip-blip" for the +20 points pick-up
    public static AudioClip Pickup => pickup != null ? pickup : pickup = Make("Pickup",
        Notes(0.05f, Wave.Square, 0.25f, 988f, 1319f));

    // Soft "puff" when slamming down: a short burst of smoothed noise
    public static AudioClip Slam => slam != null ? slam : slam = Make("Slam",
        Puff(0.22f, 0.45f));

    // Little fanfare for a new high score
    public static AudioClip NewRecord => newRecord != null ? newRecord : newRecord = Make("NewRecord",
        Notes(0.09f, Wave.Square, 0.28f, 523f, 659f, 784f, 659f, 784f, 1047f, 1047f));

    // UI click: letters of the initials, PRESS START
    public static AudioClip Blip => blip != null ? blip : blip = Make("Blip",
        Tone(880f, 880f, 0.04f, Wave.Square, 0.2f));

    // Noise through a low-pass filter that closes over time: bright "pf" at the start,
    // soft "ff" at the end, with a quick fade - like a small cloud of air
    private static float[] Puff(float seconds, float volume)
    {
        int count = Mathf.CeilToInt(seconds * Rate);
        float[] data = new float[count];
        float filtered = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            float smoothing = Mathf.Lerp(0.25f, 0.03f, t); // Smaller = darker, softer sound
            filtered += smoothing * (noise - filtered);

            float envelope = Mathf.Min(1f, i / (Rate * 0.006f)) * (1f - t) * (1f - t);
            data[i] = filtered * volume * 2.5f * envelope; // The filter makes noise quieter - bring it back up
        }
        return data;
    }

    private static float[] Notes(float noteSeconds, Wave wave, float volume, params float[] frequencies)
    {
        List<float> data = new List<float>();
        foreach (float frequency in frequencies)
        {
            data.AddRange(Tone(frequency, frequency, noteSeconds, wave, volume));
        }
        return data.ToArray();
    }

    // One sound: the pitch slides from startHz to endHz, the volume fades out at the end
    private static float[] Tone(float startHz, float endHz, float seconds, Wave wave, float volume)
    {
        int count = Mathf.CeilToInt(seconds * Rate);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            phase += Mathf.Lerp(startHz, endHz, t) / Rate;
            phase -= Mathf.Floor(phase);

            float sample;
            switch (wave)
            {
                case Wave.Square: sample = phase < 0.5f ? 1f : -1f; break;
                case Wave.Triangle: sample = 4f * Mathf.Abs(phase - 0.5f) - 1f; break;
                default: sample = (float)(random.NextDouble() * 2.0 - 1.0); break;
            }

            // Tiny fade in (no click) and a fade out over the last 40 %
            float envelope = Mathf.Min(1f, i / (Rate * 0.004f)) * Mathf.Clamp01((1f - t) / 0.4f);
            data[i] = sample * volume * envelope;
        }
        return data;
    }

    private static AudioClip Make(string name, params float[][] parts)
    {
        List<float> data = new List<float>();
        foreach (float[] part in parts) data.AddRange(part);

        AudioClip clip = AudioClip.Create(name, data.Count, 1, Rate, false);
        clip.SetData(data.ToArray(), 0);
        return clip;
    }
}
