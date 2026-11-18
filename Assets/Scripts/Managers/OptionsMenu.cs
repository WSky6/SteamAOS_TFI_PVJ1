using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Menú principal: ponerlo en el botón "Opciones" (el que está DENTRO de Main_Panel).
// Al hacer click oculta el menú principal y abre el panel de opciones con los 3 sliders de audio.
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class OptionsMenu : MonoBehaviour
{
    GameObject mainPanel;

    void Awake()
    {
        Button button = GetComponent<Button>();

        // Apaga lo que hacía el botón antes (abrir el panel viejo de una sola barra)
        int count = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        button.onClick.AddListener(Open);

        // El botón está dentro de Main_Panel: ese es el que hay que ocultar
        mainPanel = transform.parent != null ? transform.parent.gameObject : null;
    }

    void Open()
    {
        if (mainPanel != null) mainPanel.SetActive(false);

        // Fondo transparente: se ve la imagen del menú
        OptionsPanel.Open(new Color(0f, 0f, 0f, 0f), () =>
        {
            if (mainPanel != null) mainPanel.SetActive(true);
        });
    }
}
