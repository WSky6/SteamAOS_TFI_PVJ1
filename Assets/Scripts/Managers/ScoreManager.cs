using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static int Score { get; private set; }
    public static event Action<int> OnScoreChanged;

    static ScoreManager instance;
    PlayerStats stats;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Score = 0; OnScoreChanged = null; instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;

        GameObject go = new GameObject("ScoreManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScoreManager>();
    }

    void OnEnable()
    {
        EnemyEvents.OnEnemyDied += OnEnemyDied;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        EnemyEvents.OnEnemyDied -= OnEnemyDied;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnemyDied(int points, Vector3 position)
    {
        AddScore(points);
    }

    public static void AddScore(int points)
    {
        Score += points;
        Debug.Log("Puntos: " + Score + " (+" + points + ")");

        if (instance != null) instance.SaveToStats();

        OnScoreChanged?.Invoke(Score);
    }

    public static void ResetScore()
    {
        Score = 0;
        if (instance != null) instance.SaveToStats();
        OnScoreChanged?.Invoke(Score);
    }

    void SaveToStats()
    {
        if (stats == null)
        {
            PlayerStats[] all = Resources.FindObjectsOfTypeAll<PlayerStats>();
            if (all.Length > 0) stats = all[0];
        }
        if (stats != null) stats.currentScore = Score;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu")) ResetScore();
    }
}
