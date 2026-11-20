
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Juego.Tienda
{
    public class PanelDatosJugadorUI : MonoBehaviour
    {
        [Header("Datos del Jugador y Arma")]
        [SerializeField] private PlayerStats estadisticasJugador;
        [SerializeField] private WeaponData datosArma;

        [Header("Textos TMP (Opcional)")]
        [SerializeField] private TMP_Text textoVidaTMP;
        [SerializeField] private TMP_Text textoVelocidadTMP;
        [SerializeField] private TMP_Text textoBalasTMP;
        [SerializeField] private TMP_Text textoPuntajeTMP;

        [Header("Textos UI Legacy (Opcional)")]
        [SerializeField] private Text textoVidaUI;
        [SerializeField] private Text textoVelocidadUI;
        [SerializeField] private Text textoBalasUI;
        [SerializeField] private Text textoPuntajeUI;

        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += AlCambiarPuntaje;
            ActualizarPanel();
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= AlCambiarPuntaje;
        }

        private void Start()
        {
            ActualizarPanel();
        }

        private void AlCambiarPuntaje(int _)
        {
            ActualizarPanel();
        }

        public void ActualizarPanel()
        {
            float vidaActual = estadisticasJugador != null ? estadisticasJugador.currentHealth : 0f;
            float vidaMaxima = estadisticasJugador != null ? estadisticasJugador.maxHealth : 0f;
            float velMovimiento = estadisticasJugador != null ? estadisticasJugador.moveSpeed : 0f;
            float velCarrera = estadisticasJugador != null ? estadisticasJugador.sprintSpeed : 0f;
            int cargador = datosArma != null ? datosArma.magazineSize : 0;
            int puntaje = ScoreManager.Score;

            string strVida = $"Vida: {Mathf.CeilToInt(vidaActual)} / {Mathf.CeilToInt(vidaMaxima)}";
            string strVelocidad = $"Velocidad: {velMovimiento:0.#} (Sprint: {velCarrera:0.#})";
            string strBalas = $"Balas: {cargador}";
            string strPuntaje = $"Puntos: {puntaje}";

            SetearTexto(textoVidaTMP, textoVidaUI, strVida);
            SetearTexto(textoVelocidadTMP, textoVelocidadUI, strVelocidad);
            SetearTexto(textoBalasTMP, textoBalasUI, strBalas);
            SetearTexto(textoPuntajeTMP, textoPuntajeUI, strPuntaje);
        }

        private void SetearTexto(TMP_Text tmp, Text legacy, string valor)
        {
            if (tmp != null)
                tmp.text = valor;

            if (legacy != null)
                legacy.text = valor;
        }
    }
}