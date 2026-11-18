using System.Collections.Generic;
using UnityEngine;

// Sonidos del juego (disparos, enemigos, jugador).
// Los clips se cargan solos de Assets/Resources/Audio/SFX/ con el formato  nombre_1.wav, nombre_2.wav, nombre_3.wav
// Cada vez que suena un "nombre" se elige una variante al azar (distinta a la anterior) y se cambia
// un poco el tono y el volumen, así nunca suena exactamente igual.
//
// Uso:  GameAudio.Play("melee_alert", transform.position);   // sonido 3D (se escucha según la distancia)
//       GameAudio.Play2D("player_shot");                     // sonido 2D (del jugador, siempre igual de fuerte)
public static class GameAudio
{
    const int PoolSize = 24;

    static Dictionary<string, AudioClip[]> clips;
    static readonly Dictionary<string, int> last = new Dictionary<string, int>();
    static readonly HashSet<string> warned = new HashSet<string>();
    static AudioSource[] pool;
    static int next;

    // ---------------- API ----------------

    public static void Play(string name, Vector3 position, float volume = 1f, float pitchVariation = 0.08f)
    {
        AudioSource s = Prepare(name, volume, pitchVariation, out AudioClip clip, out float vol);
        if (s == null) return;

        s.transform.position = position;
        s.spatialBlend = 1f;                       // 3D
        s.rolloffMode = AudioRolloffMode.Linear;
        s.minDistance = 3f;
        s.maxDistance = 40f;
        s.PlayOneShot(clip, vol);
    }

    // Prueba de volumen (slider de Efectos): suena aunque el juego esté en pausa
    public static void Preview(string name, float volume = 0.7f)
    {
        AudioSource s = Prepare(name, volume, 0.03f, out AudioClip clip, out float vol, true);
        if (s == null) return;

        s.spatialBlend = 0f;
        s.PlayOneShot(clip, vol);
    }

    public static void Play2D(string name, float volume = 1f, float pitchVariation = 0.05f)
    {
        AudioSource s = Prepare(name, volume, pitchVariation, out AudioClip clip, out float vol);
        if (s == null) return;

        s.spatialBlend = 0f;                       // 2D
        s.PlayOneShot(clip, vol);
    }

    // ---------------- Interno ----------------

    static AudioSource Prepare(string name, float volume, float pitchVariation, out AudioClip clip, out float vol, bool ignorePause = false)
    {
        clip = null;
        vol = 0f;

        if (Time.timeScale == 0f && !ignorePause) { return null; }   // en pausa no suena nada

        LoadClips();

        if (!clips.TryGetValue(name, out AudioClip[] list) || list.Length == 0)
        {
            if (warned.Add(name))
                Debug.LogWarning("GameAudio: no encuentro sonidos '" + name + "_N' en Resources/Audio/SFX");
            return null;
        }

        // Variante al azar, distinta a la última que sonó
        int i = Random.Range(0, list.Length);
        if (list.Length > 1 && last.TryGetValue(name, out int prev) && i == prev)
            i = (i + 1) % list.Length;
        last[name] = i;
        clip = list[i];

        AudioSource s = NextSource();
        s.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        vol = volume * Random.Range(0.88f, 1f);
        return s;
    }

    static void LoadClips()
    {
        if (clips != null) return;

        clips = new Dictionary<string, AudioClip[]>();
        Dictionary<string, List<AudioClip>> temp = new Dictionary<string, List<AudioClip>>();

        foreach (AudioClip c in Resources.LoadAll<AudioClip>("Audio/SFX"))
        {
            // "melee_alert_2" -> "melee_alert"
            string key = c.name;
            int us = key.LastIndexOf('_');
            if (us > 0 && int.TryParse(key.Substring(us + 1), out _)) key = key.Substring(0, us);

            if (!temp.ContainsKey(key)) temp[key] = new List<AudioClip>();
            temp[key].Add(c);
        }

        foreach (var kv in temp) clips[kv.Key] = kv.Value.ToArray();
    }

    static AudioSource NextSource()
    {
        if (pool == null || pool[0] == null)
        {
            GameObject host = new GameObject("GameAudio");
            Object.DontDestroyOnLoad(host);
            pool = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject go = new GameObject("SFX " + i);
                go.transform.SetParent(host.transform, false);
                pool[i] = go.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
                pool[i].outputAudioMixerGroup = GameMixer.SfxGroup;   // canal SFX del mixer
            }
        }

        // Busca una libre; si todas suenan, reutiliza la más vieja
        for (int k = 0; k < PoolSize; k++)
        {
            AudioSource s = pool[(next + k) % PoolSize];
            if (!s.isPlaying) { next = (next + k + 1) % PoolSize; return s; }
        }

        AudioSource old = pool[next];
        next = (next + 1) % PoolSize;
        return old;
    }
}
