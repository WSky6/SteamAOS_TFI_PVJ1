
using UnityEngine;

namespace Juego.Tienda
{
    public class ValidadorCompra : MonoBehaviour
    {
        [Header("Aplicador de Mejoras")]
        [SerializeField] private AplicadorMejoras aplicadorMejoras;

        public int ObtenerSaldoActual()
        {
            return ScoreManager.Score;
        }

        public bool TieneSaldoSuficiente(DatosArticuloTienda articulo)
        {
            if (articulo == null)
                return false;

            return ObtenerSaldoActual() >= articulo.costoPuntos;
        }

        public bool IntentarComprar(DatosArticuloTienda articulo)
        {
            if (articulo == null)
                return false;

            if (!TieneSaldoSuficiente(articulo))
            {
                Debug.Log("Saldo insuficiente para comprar: " + articulo.titulo);
                return false;
            }

            ScoreManager.AddScore(-articulo.costoPuntos);

            if (aplicadorMejoras != null)
            {
                aplicadorMejoras.AplicarMejora(articulo);
            }

            Debug.Log("Compra realizada: " + articulo.titulo + " (-" + articulo.costoPuntos + " puntos)");
            return true;
        }
    }
}