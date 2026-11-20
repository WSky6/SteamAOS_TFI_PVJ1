
using UnityEngine;

namespace Juego.Tienda
{
    public enum TipoMejora
    {
        Vida,
        Velocidad,
        Balas
    }

    [CreateAssetMenu(fileName = "NuevoArticuloTienda", menuName = "Tienda/Articulo de Tienda")]
    public class DatosArticuloTienda : ScriptableObject
    {
        [Header("Informacion del Articulo")]
        public string titulo = "Mejora";

        [TextArea(2, 4)]
        public string descripcion = "Aumenta una estadistica del jugador.";

        public TipoMejora tipoMejora;

        [Header("Costo y Efecto")]
        [Min(1)]
        public int costoPuntos = 50;

        public float valorMejora = 20f;
    }
}