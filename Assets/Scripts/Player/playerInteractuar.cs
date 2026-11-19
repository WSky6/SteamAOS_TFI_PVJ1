using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractuar : MonoBehaviour
{
    [Header("Configuración del Raycast")]
    [Tooltip("Cámara principal desde donde se lanza la vista en primera persona.")]
    public Camera playerCamera;
    [Tooltip("Distancia máxima a la que el jugador puede interactuar.")]
    public float interactionDistance = 3f;
    [Tooltip("Capa (Layer) para optimizar el Raycast y que solo detecte interactuables.")]
    public LayerMask interactableLayer;

    [Header("Input System")]
    [Tooltip("Referencia a la acción de interacción (ej. tecla E).")]
    public InputActionReference interactAction;

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.started += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.started -= OnInteractPerformed;
            interactAction.action.Disable();
        }
    }

    private void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    Debug.Log("Mirando: " + currentInteractable.PromptMessage);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            Debug.Log("Dejó de mirar el objeto.");
            currentInteractable = null;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            Debug.Log("");
            currentInteractable.Interact();
        }
    }
}
