using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PausePlayerGuard : MonoBehaviour
{
    static PausePlayerGuard instance;

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

        Disable(player.GetComponent<FirstPersonController>());
        Disable(player.GetComponent<PlayerWeapon>());
        Disable(player.GetComponent<PlayerDash>());
    }

    void Disable(Behaviour b)
    {
        if (b == null || !b.enabled) return;
        b.enabled = false;
        disabledByPause.Add(b);
    }

    void Unfreeze()
    {
        frozen = false;

        foreach (Behaviour b in disabledByPause)
        {
            if (b == null) continue;

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
