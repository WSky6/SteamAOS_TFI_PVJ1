
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Juego.Tienda
{
    public class VerificadorPersistenciaNivel : MonoBehaviour
    {
        [Header("Datos a Verificar")]
        [SerializeField] private PlayerStats estadisticasJugador;
        [SerializeField] private WeaponData datosArma;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
        }

        private void Start()
        {
            RegistrarEstadoActual(SceneManager.GetActiveScene().name);
        }

        private void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            RegistrarEstadoActual(escena.name);
        }

        public void RegistrarEstadoActual(string nombreEscena)
        {
            int puntos = ScoreManager.Score;
            float vidaMax = estadisticasJugador != null ? estadisticasJugador.maxHealth : 0f;
            float vidaActual = estadisticasJugador != null ? estadisticasJugador.currentHealth : 0f;
            float velMov = estadisticasJugador != null ? estadisticasJugador.moveSpeed : 0f;
            float velSprint = estadisticasJugador != null ? estadisticasJugador.sprintSpeed : 0f;
            int cargador = datosArma != null ? datosArma.magazineSize : 0;

            Debug.Log(
                $"[Persistencia | {nombreEscena}] Puntos: {puntos} | " +
                $"Vida: {vidaActual}/{vidaMax} | Vel: {velMov} (Sprint: {velSprint}) | Cargador: {cargador}"
            );
        }
    }
}