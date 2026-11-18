using System;
using UnityEngine;

// Evento global: "murió un enemigo".
// Los enemigos solo avisan (RaiseDied) y no saben quién escucha: ScoreManager suma los puntos,
// y cualquier otro compañero (sonido, efectos, estadísticas) puede suscribirse sin tocar a los enemigos.
//
// Suscribirse:  EnemyEvents.OnEnemyDied += MiMetodo;   (y sacarse en OnDisable con -=)
//   void MiMetodo(int points, Vector3 position) { ... }
public static class EnemyEvents
{
    public static event Action<int, Vector3> OnEnemyDied;

    public static void RaiseDied(int points, Vector3 position)
    {
        OnEnemyDied?.Invoke(points, position);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { OnEnemyDied = null; }
}
