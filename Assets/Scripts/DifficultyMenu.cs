using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú de dificultad. Ponerlo en el botón "Jugar" del MainMenu.
// Al hacer click en Jugar aparece un panel con Fácil / Normal / Difícil / Atrás.
// Al elegir una dificultad se guarda (DifficultyScaled.SetDifficulty) y se carga el primer nivel.
// El panel se crea solo por código: no hay que armar nada más en Unity.
[RequireComponent(typeof(Button))]
public class DifficultyMenu : MonoBehaviour
{
    [SerializeField] string firstScene = "Nivel1";

    static readonly string[] names = { "Fácil", "Normal", "Difícil" };

    GameObject panel;
    Font font;

    void Awake()
    {
        Button play = GetComponent<Button>();

        // Apaga lo que hacía antes el botón Jugar (cargar Nivel1 directo)
        int count = play.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
            play.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        play.onClick.AddListener(Open);
    }

    public void Open()
    {
        if (panel == null) BuildPanel();
        panel.SetActive(true);
    }

    void Close()
    {
        if (panel != null) panel.SetActive(false);
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
        canvasGo.AddComponent<GraphicRaycaster>();
        panel = canvasGo;

        // Fondo oscuro que tapa el menú
        Image bg = NewImage("Fondo", canvasGo.transform, new Color(0f, 0f, 0f, 0.85f));
        Stretch(bg.rectTransform);

        NewLabel("Titulo", bg.transform, "Elegí la dificultad", 60, new Vector2(0, 280), new Vector2(900, 100));

        int current = DifficultyScaled.CurrentIndex;
        for (int i = 0; i < names.Length; i++)
        {
            int index = i;
            string text = names[i] + (i == current ? "  (última elegida)" : "");
            NewButton(text, bg.transform, new Vector2(0, 130 - i * 130), () => Choose(index));
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
        rt.sizeDelta = new Vector2(600, 100);

        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);

        Text t = NewLabel("Texto", img.transform, text, 48, Vector2.zero, rt.sizeDelta);
        t.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }
}
