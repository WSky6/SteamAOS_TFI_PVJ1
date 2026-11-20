
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Juego.Tienda
{
    public class TarjetaArticuloTienda : MonoBehaviour
    {
        [Header("Referencias de Tienda y Articulo")]
        [SerializeField] private ControladorTienda controladorTienda;
        [SerializeField] private DatosArticuloTienda articulo;

        [Header("Controles UI")]
        [SerializeField] private Button botonComprar;
        [SerializeField] private TMP_Text textoTituloTMP;
        [SerializeField] private TMP_Text textoDescripcionTMP;
        [SerializeField] private TMP_Text textoCostoTMP;
        [SerializeField] private TMP_Text textoBotonTMP;

        [Header("Textos UI Legacy (Opcional)")]
        [SerializeField] private Text textoTituloUI;
        [SerializeField] private Text textoDescripcionUI;
        [SerializeField] private Text textoCostoUI;
        [SerializeField] private Text textoBotonUI;

        private void Awake()
        {
            if (botonComprar != null)
            {
                botonComprar.onClick.AddListener(AlPulsarComprar);
            }
        }

        private void OnEnable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnValoresTiendaActualizados += RefrescarTarjeta;
            }

            ScoreManager.OnScoreChanged += AlCambiarPuntaje;
            RefrescarTarjeta();
        }

        private void OnDisable()
        {
            if (controladorTienda != null)
            {
                controladorTienda.OnValoresTiendaActualizados -= RefrescarTarjeta;
            }

            ScoreManager.OnScoreChanged -= AlCambiarPuntaje;
        }

        private void Start()
        {
            RefrescarTarjeta();
        }

        private void AlCambiarPuntaje(int _)
        {
            RefrescarTarjeta();
        }

        private void AlPulsarComprar()
        {
            if (controladorTienda == null || articulo == null)
                return;

            controladorTienda.ComprarArticulo(articulo);
            RefrescarTarjeta();
        }

        public void RefrescarTarjeta()
        {
            if (articulo == null)
                return;

            string tituloStr = string.IsNullOrWhiteSpace(articulo.titulo)
                ? articulo.tipoMejora.ToString()
                : articulo.titulo;
            string descStr = articulo.descripcion;
            string costoStr = $"Costo: {articulo.costoPuntos} pts";

            SetearTexto(textoTituloTMP, textoTituloUI, tituloStr);
            SetearTexto(textoDescripcionTMP, textoDescripcionUI, descStr);
            SetearTexto(textoCostoTMP, textoCostoUI, costoStr);

            bool puedeComprar = controladorTienda != null && controladorTienda.PuedeComprar(articulo);

            if (botonComprar != null)
            {
                botonComprar.interactable = puedeComprar;
            }

            string textoBtn = puedeComprar ? "Comprar" : "Sin puntos";
            SetearTexto(textoBotonTMP, textoBotonUI, textoBtn);
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