
using System;
using UnityEngine;

namespace Juego.Tienda
{
    public class ControladorTienda : MonoBehaviour
    {
        [Header("Referencias de Tienda")]
        [SerializeField] private ValidadorCompra validadorCompra;
        [SerializeField] private GameObject panelTienda;
        [SerializeField] private DatosArticuloTienda[] articulosDisponibles;

        public event Action<bool> OnEstadoTiendaCambiado;
        public event Action<string, bool> OnResultadoCompra;
        public event Action OnValoresTiendaActualizados;

        public DatosArticuloTienda[] ArticulosDisponibles => articulosDisponibles;
        public bool EstaAbierta => panelTienda != null && panelTienda.activeSelf;

        private void Awake()
        {
            if (panelTienda != null)
            {
                panelTienda.SetActive(false);
            }
        }

        public void AbrirTienda()
        {
            if (panelTienda != null)
            {
                panelTienda.SetActive(true);
            }

            ConfigurarCursor(true);
            OnEstadoTiendaCambiado?.Invoke(true);
            OnValoresTiendaActualizados?.Invoke();
        }

        public void CerrarTienda()
        {
            if (panelTienda != null)
            {
                panelTienda.SetActive(false);
            }

            ConfigurarCursor(false);
            OnEstadoTiendaCambiado?.Invoke(false);
        }

        public void AlternarTienda()
        {
            if (EstaAbierta)
            {
                CerrarTienda();
            }
            else
            {
                AbrirTienda();
            }
        }

        public bool PuedeComprar(DatosArticuloTienda articulo)
        {
            return validadorCompra != null && validadorCompra.TieneSaldoSuficiente(articulo);
        }

        public bool ComprarArticulo(DatosArticuloTienda articulo)
        {
            if (articulo == null)
            {
                OnResultadoCompra?.Invoke("Artículo no válido.", false);
                return false;
            }

            if (validadorCompra == null)
            {
                OnResultadoCompra?.Invoke("Falta configurar ValidadorCompra.", false);
                return false;
            }

            bool compraExitosa = validadorCompra.IntentarComprar(articulo);

            if (compraExitosa)
            {
                OnResultadoCompra?.Invoke("Compraste: " + articulo.titulo, true);
                OnValoresTiendaActualizados?.Invoke();
            }
            else
            {
                OnResultadoCompra?.Invoke("Puntos insuficientes para: " + articulo.titulo, false);
            }

            return compraExitosa;
        }

        private void ConfigurarCursor(bool tiendaAbierta)
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