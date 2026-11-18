using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú de dificultad. Ponerlo en el botón "Jugar" del MainMenu.
// Al hacer click en Jugar aparece un panel con Fácil / Normal / Difícil / Atrás.
// Al elegir una dificultad se guarda (DifficultyScaled.SetDifficulty) y se carga el primer nivel.
// El panel se crea solo por código: no hay que armar nada más en Unity.
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class DifficultyMenu : MonoBehaviour
{
    [SerializeField] string firstScene = "Nivel1";

    static readonly string[] names = { "Fácil", "Normal", "Difícil" };

    // Compartido: aunque haya dos copias del script, el panel se crea una sola vez
    static GameObject panel;
    GameObject mainPanel;   // el panel del menú principal (Main_Panel), se oculta como hace Opciones
    Font font;

    void Awake()
    {
        Button play = GetComponent<Button>();

        // Apaga lo que hacía antes el botón Jugar (cargar Nivel1 directo)
        int count = play.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
            play.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        play.onClick.AddListener(Open);

        // El botón Play está dentro de Main_Panel: ese es el que hay que ocultar
        mainPanel = transform.parent != null ? transform.parent.gameObject : null;
    }

    public void Open()
    {
        if (panel == null) BuildPanel();
        if (mainPanel != null) mainPanel.SetActive(false);
        panel.SetActive(true);
    }

    void Close()
    {
        if (panel != null) panel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    void Choose(int index)
    {
        DifficultyScaled.SetDifficulty(index);
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstScene);
    }

    // ---------------- UI creada por código ----------------

    void BuildPanel()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasGo = new GameObject("DifficultyCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;   // se ajusta al alto: entra en cualquier pantalla
        canvasGo.AddComponent<GraphicRaycaster>();
        panel = canvasGo;

        // Fondo transparente: se ve la imagen del menú, igual que en Opciones
        Image bg = NewImage("Fondo", canvasGo.transform, new Color(0f, 0f, 0f, 0f));
        bg.raycastTarget = false;
        Stretch(bg.rectTransform);

        Text title = NewLabel("Titulo", bg.transform, "DIFICULTAD", 90, new Vector2(0, 380), new Vector2(1200, 140));
        title.fontStyle = FontStyle.Bold;

        int current = DifficultyScaled.CurrentIndex;
        for (int i = 0; i < names.Length; i++)
        {
            int index = i;
            string text = names[i] + (i == current ? "  (última elegida)" : "");
            NewButton(text, bg.transform, new Vector2(0, 160 - i * 120), () => Choose(index));
        }

        NewButton("Atrás", bg.transform, new Vector2(0, -290), Close);
    }

    Image NewImage(string n, Transform parent, Color c)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = c;
        return img;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    Text NewLabel(string n, Transform parent, string text, int size, Vector2 pos, Vector2 dim)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        RectTransform rt = t.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;
        return t;
    }

    void NewButton(string text, Transform parent, Vector2 pos, UnityAction onClick)
    {
        Image img = NewImage("Btn " + text, parent, new Color(0.95f, 0.95f, 0.95f, 1f));
        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 90);

        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);

        Text t = NewLabel("Texto", img.transform, text, 48, Vector2.zero, rt.sizeDelta);
        t.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }
}
