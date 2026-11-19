using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class OptionsMenu : MonoBehaviour
{
    GameObject mainPanel;

    void Awake()
    {
        Button button = GetComponent<Button>();

        int count = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        button.onClick.AddListener(Open);

        mainPanel = transform.parent != null ? transform.parent.gameObject : null;
    }

    void Open()
    {
        if (mainPanel != null) mainPanel.SetActive(false);

        OptionsPanel.Open(new Color(0f, 0f, 0f, 0f), () =>
        {
            if (mainPanel != null) mainPanel.SetActive(true);
        });
    }
}
