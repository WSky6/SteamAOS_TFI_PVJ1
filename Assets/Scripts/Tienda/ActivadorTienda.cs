
using UnityEngine;
using UnityEngine.UI;

namespace Juego.Tienda
{
    public class ActivadorTienda : MonoBehaviour, IInteractable
    {
        [Header("Controlador de Tienda")]
        [SerializeField] private ControladorTienda controladorTienda;

        [Header("Botón de UI (Opcional)")]
        [SerializeField] private Button botonAbrirCerrarUI;

        [Header("Mensajes de Terminal")]
        [SerializeField] private string mensajeAbrir = "Presiona E para abrir la tienda";
        [SerializeField] private string mensajeCerrar = "Presiona E para cerrar la tienda";

        [Header("Estado de Juego (Opcional)")]
        [SerializeField] private bool cambiarEstadoJuego = true;

        public string PromptMessage =>
            controladorTienda != null && controladorTienda.EstaAbierta
                ? mensajeCerrar
                : mensajeAbrir;

        private void Awake()
        {
            if (botonAbrirCerrarUI != null)
            {
                botonAbrirCerrarUI.onClick.AddListener(AlternarTienda);
            }
        }

        private void OnEnable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnEstadoTiendaCambiado += AlCambiarEstadoTienda;
            }
        }

        private void OnDisable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnEstadoTiendaCambiado -= AlCambiarEstadoTienda;
            }
        }

        public void Interact()
        {
            AlternarTienda();
        }

        public void AbrirTienda()
        {
            if (controladorTienda == null)
                return;

            controladorTienda.AbrirTienda();
        }

        public void CerrarTienda()
        {
            if (controladorTienda == null)
                return;

            controladorTienda.CerrarTienda();
        }

        public void AlternarTienda()
        {
            if (controladorTienda == null)
                return;

            controladorTienda.AlternarTienda();
        }

        private void AlCambiarEstadoTienda(bool tiendaAbierta)
        {
            AplicarEstadoCursor(tiendaAbierta);

            if (!cambiarEstadoJuego)
                return;

            GameState nuevoEstado = tiendaAbierta ? GameState.Paused : GameState.Playing;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(nuevoEstado);
            }
        }

        private void AplicarEstadoCursor(bool tiendaAbierta)
        {
            if (tiendaAbierta)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}