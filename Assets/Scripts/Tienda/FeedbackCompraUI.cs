
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Juego.Tienda
{
    public class FeedbackCompraUI : MonoBehaviour
    {
        [Header("Referencias de Tienda y UI")]
        [SerializeField] private ControladorTienda controladorTienda;
        [SerializeField] private PanelDatosJugadorUI panelDatosJugador;
        [SerializeField] private TarjetaArticuloTienda[] tarjetasArticulos;

        [Header("Contenedor y Texto TMP (Opcional)")]
        [SerializeField] private GameObject contenedorMensaje;
        [SerializeField] private TMP_Text textoMensajeTMP;

        [Header("Texto UI Legacy (Opcional)")]
        [SerializeField] private Text textoMensajeUI;

        [Header("Colores y Duración")]
        [SerializeField] private Color colorExito = new Color(0.2f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color colorError = new Color(0.95f, 0.25f, 0.25f, 1f);
        [SerializeField] private float duracionMensaje = 2.5f;
        [SerializeField] private bool ocultarAlTerminar = true;

        private float temporizadorOcultar;
        private bool mensajeActivo;

        private void Awake()
        {
            OcultarFeedbackInmediato();
        }

        private void OnEnable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnResultadoCompra += AlRecibirResultadoCompra;
                controladorTienda.OnValoresTiendaActualizados += RefrescarUI;
            }
        }

        private void OnDisable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnResultadoCompra -= AlRecibirResultadoCompra;
                controladorTienda.OnValoresTiendaActualizados -= RefrescarUI;
            }
        }

        private void Update()
        {
            if (!mensajeActivo || !ocultarAlTerminar)
                return;

            temporizadorOcultar -= Time.unscaledDeltaTime;
            if (temporizadorOcultar <= 0f)
            {
                OcultarFeedbackInmediato();
            }
        }

        private void AlRecibirResultadoCompra(string mensaje, bool exitosa)
        {
            Color colorAplicado = exitosa ? colorExito : colorError;

            if (contenedorMensaje != null)
            {
                contenedorMensaje.SetActive(true);
            }

            if (textoMensajeTMP != null)
            {
                textoMensajeTMP.text = mensaje;
                textoMensajeTMP.color = colorAplicado;
            }

            if (textoMensajeUI != null)
            {
                textoMensajeUI.text = mensaje;
                textoMensajeUI.color = colorAplicado;
            }

            temporizadorOcultar = duracionMensaje;
            mensajeActivo = true;

            RefrescarUI();
        }

        public void RefrescarUI()
        {
            if (panelDatosJugador != null)
            {
                panelDatosJugador.ActualizarPanel();
            }

            if (tarjetasArticulos == null)
                return;

            for (int i = 0; i < tarjetasArticulos.Length; i++)
            {
                if (tarjetasArticulos[i] != null)
                {
                    tarjetasArticulos[i].RefrescarTarjeta();
                }
            }
        }

        public void OcultarFeedbackInmediato()
        {
            mensajeActivo = false;

            if (contenedorMensaje != null)
            {
                contenedorMensaje.SetActive(false);
            }

            if (textoMensajeTMP != null)
            {
                textoMensajeTMP.text = string.Empty;
            }

            if (textoMensajeUI != null)
            {
                textoMensajeUI.text = string.Empty;
            }
        }
    }
}