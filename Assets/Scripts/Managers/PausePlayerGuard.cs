using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

// Congela los controles del jugador mientras el juego está detenido (pausa, tutorial, victoria, game over).
// Sin esto, con Time.timeScale = 0 la cámara seguía girando con el mouse y el arma podía disparar al hacer clic en el menú.
// Se crea solo al iniciar el juego: no hay que ponerlo en ninguna escena.
public class PausePlayerGuard : MonoBehaviour
{
    static PausePlayerGuard instance;

    // Componentes que se apagaron por la pausa (para volver a encender solo esos)
    readonly List<Behaviour> disabledByPause = new List<Behaviour>();
    bool frozen;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;

        GameObject go = new GameObject("PausePlayerGuard");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<PausePlayerGuard>();
    }

    void Update()
    {
        bool stopped = Time.timeScale == 0f;

        if (stopped && !frozen) Freeze();
        else if (!stopped && frozen) Unfreeze();
    }

    void Freeze()
    {
        frozen = true;
        disabledByPause.Clear();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Movimiento y cámara, arma y dash
        Disable(player.GetComponent<FirstPersonController>());
        Disable(player.GetComponent<PlayerWeapon>());
        Disable(player.GetComponent<PlayerDash>());
    }

    void Disable(Behaviour b)
    {
        if (b == null || !b.enabled) return;   // si ya estaba apagado (ej. arma muerta) no se toca
        b.enabled = false;
        disabledByPause.Add(b);
    }

    void Unfreeze()
    {
        frozen = false;

        foreach (Behaviour b in disabledByPause)
        {
            if (b == null) continue;   // el objeto se destruyó (cambio de escena)

            // Si el jugador murió mientras tanto, el arma tiene que seguir apagada
            if (b is PlayerWeapon)
            {
                PlayerHealth health = b.GetComponent<PlayerHealth>();
                if (health != null && health.IsDead) continue;
            }

            b.enabled = true;
        }
        disabledByPause.Clear();
    }
}
