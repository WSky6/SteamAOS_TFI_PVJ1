using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Panel de opciones de audio: 3 sliders (General, Música, Efectos) conectados por script al AudioMixer.
// Se crea por código, así se puede abrir desde el Menú Principal y desde el Menú de Pausa
// sin armar nada en las escenas:  OptionsPanel.Open(colorDeFondo, alCerrar);
public static class OptionsPanel
{
    static GameObject root;
    static System.Action onClose;
    static Font font;

    public static bool IsOpen { get { return root != null && root.activeSelf; } }

    public static void Open(Color background, System.Action closed = null)
    {
        onClose = closed;

        if (root != null) Object.Destroy(root);   // se rearma para tomar los volúmenes guardados
        Build(background);
        root.SetActive(true);
    }

    public static void Close()
    {
        if (root != null) root.SetActive(false);

        System.Action cb = onClose;
        onClose = null;
        if (cb != null) cb();
    }

    // ---------------- Construcción de la UI ----------------

    static void Build(Color background)
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        root = new GameObject("OptionsCanvas");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        root.AddComponent<GraphicRaycaster>();

        Image bg = NewImage("Fondo", root.transform, background);
        Stretch(bg.rectTransform);

        Text title = NewLabel("Titulo", bg.transform, "OPCIONES", 90, new Vector2(0, 380), new Vector2(1200, 140));
        title.fontStyle = FontStyle.Bold;

        NewSlider("Volumen general", GameMixer.Master, bg.transform, 170f, null);
        NewSlider("Música", GameMixer.Music, bg.transform, 20f, null);
        NewSlider("Efectos", GameMixer.Sfx, bg.transform, -130f, "player_shot");   // al soltar suena una prueba

        NewButton("Atrás", bg.transform, new Vector2(0, -320), Close);
    }

    static void NewSlider(string label, string param, Transform parent, float y, string previewSound)
    {
        NewLabel("Label " + label, parent, label, 48, new Vector2(0, y + 50), new Vector2(800, 70));

        GameObject go = new GameObject("Slider " + label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = new Vector2(0, y - 10);
        rt.sizeDelta = new Vector2(700, 40);

        // Fondo de la barra
        Image back = NewImage("Background", go.transform, new Color(0.15f, 0.15f, 0.15f, 0.9f));
        back.rectTransform.anchorMin = new Vector2(0, 0.25f);
        back.rectTransform.anchorMax = new Vector2(1, 0.75f);
        back.rectTransform.offsetMin = Vector2.zero;
        back.rectTransform.offsetMax = Vector2.zero;

        // Parte rellena
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fa = (RectTransform)fillArea.transform;
        fa.anchorMin = new Vector2(0, 0.25f);
        fa.anchorMax = new Vector2(1, 0.75f);
        fa.offsetMin = new Vector2(10, 0);
        fa.offsetMax = new Vector2(-10, 0);

        Image fill = NewImage("Fill", fillArea.transform, new Color(0.85f, 0.2f, 0.25f, 1f));
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        // Manija
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        RectTransform ha = (RectTransform)handleArea.transform;
        ha.anchorMin = Vector2.zero;
        ha.anchorMax = Vector2.one;
        ha.offsetMin = new Vector2(15, 0);
        ha.offsetMax = new Vector2(-15, 0);

        Image handle = NewImage("Handle", handleArea.transform, new Color(0.95f, 0.95f, 0.95f, 1f));
        handle.rectTransform.sizeDelta = new Vector2(30, 0);
        handle.rectTransform.anchorMin = new Vector2(0, 0);
        handle.rectTransform.anchorMax = new Vector2(0, 1);

        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = GameMixer.Get(param);

        // El slider mueve el volumen del canal del AudioMixer
        slider.onValueChanged.AddListener(v => GameMixer.Set(param, v));

        // Al soltar el slider suena un ejemplo (en pausa los efectos no suenan solos)
        if (!string.IsNullOrEmpty(previewSound))
        {
            EventTrigger trigger = go.AddComponent<EventTrigger>();
            EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => GameAudio.Preview(previewSound));
            trigger.triggers.Add(up);
        }
    }

    static Image NewImage(string n, Transform parent, Color c)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = c;
        return img;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Text NewLabel(string n, Transform parent, string text, int size, Vector2 pos, Vector2 dim)
    {
        GameObject go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        t.rectTransform.anchoredPosition = pos;
        t.rectTransform.sizeDelta = dim;
        return t;
    }

    static void NewButton(string text, Transform parent, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        Image img = NewImage("Btn " + text, parent, new Color(0.95f, 0.95f, 0.95f, 1f));
        img.rectTransform.anchoredPosition = pos;
        img.rectTransform.sizeDelta = new Vector2(600, 90);

        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);

        Text t = NewLabel("Texto", img.transform, text, 48, Vector2.zero, img.rectTransform.sizeDelta);
        t.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }
}
