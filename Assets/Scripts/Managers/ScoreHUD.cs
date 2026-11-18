using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Muestra los puntos en pantalla (arriba al centro). Escucha el evento ScoreManager.OnScoreChanged.
// Se crea solo al iniciar el juego y aparece únicamente en los niveles (no en el menú).
// Si el HUD de un compañero ya tiene su propio texto de puntos, alcanza con que se suscriba a
// ScoreManager.OnScoreChanged y se puede borrar este script.
public class ScoreHUD : MonoBehaviour
{
    static ScoreHUD instance;

    GameObject canvasObj;
    Text label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;

        GameObject go = new GameObject("ScoreHUD");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScoreHUD>();
    }

    void Awake()
    {
        Build();
        Refresh(ScoreManager.Score);
    }

    void OnEnable()
    {
        ScoreManager.OnScoreChanged += Refresh;
    }

    void OnDisable()
    {
        ScoreManager.OnScoreChanged -= Refresh;
    }

    void Update()
    {
        // Solo se ve en los niveles
        bool inLevel = !SceneManager.GetActiveScene().name.Contains("Menu");
        if (canvasObj != null && canvasObj.activeSelf != inLevel) canvasObj.SetActive(inLevel);
    }

    void Refresh(int score)
    {
        if (label != null) label.text = "Puntos: " + score;
    }

    void Build()
    {
        canvasObj = new GameObject("ScoreCanvas");
        canvasObj.transform.SetParent(transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        GameObject go = new GameObject("Puntos", typeof(RectTransform));
        go.transform.SetParent(canvasObj.transform, false);

        label = go.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 56;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.UpperCenter;
        label.color = Color.white;
        label.raycastTarget = false;

        // Borde negro para que se lea sobre cualquier fondo
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        RectTransform rt = label.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -30f);
        rt.sizeDelta = new Vector2(700f, 90f);
    }
}
