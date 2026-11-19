using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    [Header("Música por escena (opcional: vacío = se usa el clip que ya tiene el AudioSource)")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Volumen según el estado del juego")]
    [SerializeField, Range(0f, 1f)] private float playingVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float pausedVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float tutorialVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float endVolume = 0.2f;
    [SerializeField] private float fadeSpeed = 2f;

    private AudioSource source;
    private GameManager subscribedTo;
    private float targetVolume = 1f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponentInChildren<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        var group = GameMixer.MusicGroup;
        if (group != null) source.outputAudioMixerGroup = group;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();
    }

    private void Update()
    {
        if (instance != this || source == null) return;

        TrySubscribe();

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, fadeSpeed * Time.unscaledDeltaTime);
    }

    private void TrySubscribe()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm == subscribedTo) return;

        Unsubscribe();
        gm.OnChangeState += OnStateChanged;
        subscribedTo = gm;
        OnStateChanged(gm.CurrentState);
    }

    private void Unsubscribe()
    {
        if (subscribedTo != null) subscribedTo.OnChangeState -= OnStateChanged;
        subscribedTo = null;
    }

    private void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:   targetVolume = pausedVolume; break;
            case GameState.Tutorial: targetVolume = tutorialVolume; break;
            case GameState.Victory:
            case GameState.GameOver: targetVolume = endVolume; break;
            default:                 targetVolume = playingVolume; break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != this || source == null) return;

        AudioClip wanted = scene.name.Contains("Menu") ? menuMusic : gameMusic;
        if (wanted != null && source.clip != wanted)
        {
            source.clip = wanted;
            source.Play();
        }
    }
}
