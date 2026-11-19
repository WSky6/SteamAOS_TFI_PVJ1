using UnityEngine;
using UnityEngine.Audio;

public static class GameMixer
{
    public const string Master = "MasterVolume";
    public const string Music = "MusicVolume";
    public const string Sfx = "SFXVolume";

    static AudioMixer mixer;
    static bool loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { mixer = null; loaded = false; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ApplySaved()
    {
        Apply(Master, Get(Master));
        Apply(Music, Get(Music));
        Apply(Sfx, Get(Sfx));
    }

    public static AudioMixer Mixer
    {
        get
        {
            if (!loaded)
            {
                loaded = true;
                mixer = Resources.Load<AudioMixer>("Audio/MainMixer");
                if (mixer == null)
                    Debug.LogWarning("GameMixer: no encuentro Resources/Audio/MainMixer (mové el mixer a Assets/Scripts/Resources/Audio/)");
            }
            return mixer;
        }
    }

    public static AudioMixerGroup MusicGroup { get { return Group("Master/Music"); } }
    public static AudioMixerGroup SfxGroup { get { return Group("Master/SFX"); } }

    static AudioMixerGroup Group(string path)
    {
        AudioMixer m = Mixer;
        if (m == null) return null;

        AudioMixerGroup[] found = m.FindMatchingGroups(path);
        return found != null && found.Length > 0 ? found[0] : null;
    }

    public static float Get(string param)
    {
        return PlayerPrefs.GetFloat(param, 1f);
    }

    public static void Set(string param, float value01)
    {
        value01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(param, value01);
        Apply(param, value01);
    }

    static void Apply(string param, float value01)
    {
        AudioMixer m = Mixer;
        if (m == null) return;

        m.SetFloat(param, Mathf.Log10(Mathf.Max(value01, 0.0001f)) * 20f);
    }
}
