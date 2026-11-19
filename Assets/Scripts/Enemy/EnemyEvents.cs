using System;
using UnityEngine;

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
