using UnityEngine;
using UnityEngine.UI;

// Menú de pausa: agrega un botón "Opciones" que aparece SOLO mientras el juego está en pausa
// y abre el mismo panel de opciones (3 sliders de audio). Escucha el evento OnChangeState del GameManager.
// Se crea solo al iniciar el juego: no hay que ponerlo en ninguna escena.
public class PauseOptions : MonoBehaviour
{
    static PauseOptions instance;

    GameObject buttonCanvas;
    GameManager subscribedTo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;

        GameObject go = new GameObject("PauseOptions");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<PauseOptions>();
    }

    void Awake()
    {
        BuildButton();
        buttonCanvas.SetActive(false);
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Update()
    {
        // El GameManager aparece recién en los niveles: apenas existe, nos suscribimos
        GameManager gm = GameManager.Instance;
        if (gm != null && gm != subscribedTo)
        {
            Unsubscribe();
            gm.OnChangeState += OnStateChanged;
            subscribedTo = gm;
            OnStateChanged(gm.CurrentState);
        }
    }

    void Unsubscribe()
    {
        if (subscribedTo != null) subscribedTo.OnChangeState -= OnStateChanged;
        subscribedTo = null;
    }

    // Evento del GameManager: el botón se ve solo en pausa
    void OnStateChanged(GameState state)
    {
        bool paused = state == GameState.Paused;
        if (buttonCanvas != null) buttonCanvas.SetActive(paused);

        if (!paused && OptionsPanel.IsOpen) OptionsPanel.Close();
    }

    void BuildButton()
    {
        buttonCanvas = new GameObject("PauseOptionsCanvas");
        buttonCanvas.transform.SetParent(transform, false);

        Canvas canvas = buttonCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = buttonCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        buttonCanvas.AddComponent<GraphicRaycaster>();

        // Botón arriba a la derecha
        GameObject btn = new GameObject("Opciones", typeof(RectTransform));
        btn.transform.SetParent(buttonCanvas.transform, false);
        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-40, -40);
        rt.sizeDelta = new Vector2(380, 90);

        Button b = btn.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(() => OptionsPanel.Open(new Color(0f, 0f, 0f, 0.9f)));

        GameObject label = new GameObject("Texto", typeof(RectTransform));
        label.transform.SetParent(btn.transform, false);
        Text t = label.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = "Opciones";
        t.fontSize = 48;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        t.raycastTarget = false;
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
    }
}
