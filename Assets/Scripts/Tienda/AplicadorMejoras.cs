
using UnityEngine;

namespace Juego.Tienda
{
    public class AplicadorMejoras : MonoBehaviour
    {
        [Header("Datos del Jugador y del Arma")]
        [SerializeField] private PlayerStats estadisticasJugador;
        [SerializeField] private WeaponData datosArma;

        [Header("Ajuste de Velocidad")]
        [SerializeField] private bool ajustarVelocidadCarrera = true;

        public void AplicarMejora(DatosArticuloTienda articulo)
        {
            if (articulo == null)
                return;

            switch (articulo.tipoMejora)
            {
                case TipoMejora.Vida:
                    AplicarMejoraVida(articulo.valorMejora);
                    break;

                case TipoMejora.Velocidad:
                    AplicarMejoraVelocidad(articulo.valorMejora);
                    break;

                case TipoMejora.Balas:
                    AplicarMejoraBalas(articulo.valorMejora);
                    break;
            }
        }

        private void AplicarMejoraVida(float cantidad)
        {
            if (estadisticasJugador == null)
                return;

            estadisticasJugador.maxHealth += cantidad;
            estadisticasJugador.currentHealth = Mathf.Min(
                estadisticasJugador.maxHealth,
                estadisticasJugador.currentHealth + cantidad
            );
        }

        private void AplicarMejoraVelocidad(float cantidad)
        {
            if (estadisticasJugador == null)
                return;

            estadisticasJugador.moveSpeed += cantidad;

            if (ajustarVelocidadCarrera)
            {
                estadisticasJugador.sprintSpeed += cantidad;
            }
        }

        private void AplicarMejoraBalas(float cantidad)
        {
            if (datosArma == null)
                return;

            int cantidadBalas = Mathf.RoundToInt(cantidad);
            datosArma.magazineSize += cantidadBalas;
        }
    }
}