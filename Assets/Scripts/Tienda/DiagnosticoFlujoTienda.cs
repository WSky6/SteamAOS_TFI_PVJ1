
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Juego.Tienda
{
    public class DiagnosticoFlujoTienda : MonoBehaviour
    {
        [Header("Referencias del Sistema de Tienda")]
        [SerializeField] private ControladorTienda controladorTienda;
        [SerializeField] private PlayerStats estadisticasJugador;
        [SerializeField] private WeaponData datosArma;

        [Header("Ajustes de Diagnóstico")]
        [SerializeField] private bool registrarAlCargarEscena = true;
        [SerializeField] private bool comprobarReferenciasEnStart = true;

        private void OnEnable()
        {
            if (registrarAlCargarEscena)
            {
                SceneManager.sceneLoaded += AlCargarEscena;
            }

            if (controladorTienda != null)
            {
                controladorTienda.OnEstadoTiendaCambiado += AlCambiarEstadoTienda;
                controladorTienda.OnResultadoCompra += AlRecibirResultadoCompra;
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;

            if (controladorTienda != null)
            {
                controladorTienda.OnEstadoTiendaCambiado -= AlCambiarEstadoTienda;
                controladorTienda.OnResultadoCompra -= AlRecibirResultadoCompra;
            }
        }

        private void Start()
        {
            if (comprobarReferenciasEnStart)
            {
                ValidarReferencias();
            }

            if (registrarAlCargarEscena)
            {
                ImprimirResumenEstado("Inicio de escena: " + SceneManager.GetActiveScene().name);
            }
        }

        private void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            ImprimirResumenEstado("Escena cargada: " + escena.name);
        }

        private void AlCambiarEstadoTienda(bool abierta)
        {
            string estado = abierta ? "ABIERTA" : "CERRADA";
            ImprimirResumenEstado("Tienda " + estado);
        }

        private void AlRecibirResultadoCompra(string mensaje, bool exitosa)
        {
            string etiqueta = exitosa ? "COMPRA OK" : "SIN SALDO / ERROR";
            ImprimirResumenEstado(etiqueta + " -> " + mensaje);
        }

        public void ValidarReferencias()
        {
            if (controladorTienda == null)
            {
                Debug.LogWarning("[Tienda | Diagnóstico] Falta asignar ControladorTienda.");
            }

            if (estadisticasJugador == null)
            {
                Debug.LogWarning("[Tienda | Diagnóstico] Falta asignar PlayerStats.");
            }

            if (datosArma == null)
            {
                Debug.LogWarning("[Tienda | Diagnóstico] Falta asignar WeaponData.");
            }

            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[Tienda | Diagnóstico] GameManager.Instance no está activo en la escena.");
            }
        }

        public void ImprimirResumenEstado(string contexto)
        {
            int puntos = ScoreManager.Score;
            int saldoStats = estadisticasJugador != null ? estadisticasJugador.currentScore : 0;
            float vidaActual = estadisticasJugador != null ? estadisticasJugador.currentHealth : 0f;
            float vidaMax = estadisticasJugador != null ? estadisticasJugador.maxHealth : 0f;
            float velMov = estadisticasJugador != null ? estadisticasJugador.moveSpeed : 0f;
            float velSprint = estadisticasJugador != null ? estadisticasJugador.sprintSpeed : 0f;
            int cargador = datosArma != null ? datosArma.magazineSize : 0;
            float danoArma = datosArma != null ? datosArma.damage : 0f;

            Debug.Log(
                $"[Tienda | {contexto}] Puntos HUD: {puntos} (Stats: {saldoStats}) | " +
                $"Vida: {vidaActual:0.#}/{vidaMax:0.#} | Vel: {velMov:0.#}/{velSprint:0.#} | " +
                $"Cargador: {cargador} | Daño: {danoArma:0.#}"
            );
        }
    }
}
