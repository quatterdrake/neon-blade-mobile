using UnityEngine;

public enum VolumeKind { Master, Music, SFX }

// Slider value (0.0001..1) <-> mixer decibels, stored in the week 11 save file.
public static class VolumeSettings
{
    public const float Min = 0.0001f;   // Log10(0.0001) * 20 = -80 dB = silence

    static string Param(VolumeKind k)
    {
        switch (k)
        {
            case VolumeKind.Master: return "MasterVol";
            case VolumeKind.Music:  return "MusicVol";
            default:                return "SFXVol";
        }
    }

    // the ear hears ratios, so the fader is logarithmic: 1 -> 0 dB, 0.5 -> -6 dB, 0.1 -> -20 dB
    public static float ToDecibels(float v) => Mathf.Log10(Mathf.Max(v, Min)) * 20f;

    public static float Get(VolumeKind k)
    {
        var d = SaveManager.Data;
        switch (k)
        {
            case VolumeKind.Master: return d.masterVolume;
            case VolumeKind.Music:  return d.musicVolume;
            default:                return d.sfxVolume;
        }
    }

    public static void Set(VolumeKind k, float value)
    {
        var d = SaveManager.Data;
        switch (k)
        {
            case VolumeKind.Master: d.masterVolume = value; break;
            case VolumeKind.Music:  d.musicVolume = value; break;
            default:                d.sfxVolume = value; break;
        }
        Apply(k);   // written to disk when the panel closes (SettingsPanel.Hide)
    }

    public static void ApplyAll()
    {
        Apply(VolumeKind.Master);
        Apply(VolumeKind.Music);
        Apply(VolumeKind.SFX);
    }

    static void Apply(VolumeKind k)
    {
        var mixer = AudioManager.Instance != null ? AudioManager.Instance.Mixer : null;
        if (mixer == null) return;
        if (!mixer.SetFloat(Param(k), ToDecibels(Get(k))))
            Debug.LogWarning("VolumeSettings: exposed parameter '" + Param(k) + "' not found in the mixer");
    }
}
