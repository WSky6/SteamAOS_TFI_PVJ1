using System.Collections;
using UnityEngine;

public class AnimationInstaller : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (FindAnyObjectByType<AnimationInstaller>() != null) return;

        GameObject go = new GameObject("AnimationInstaller");
        DontDestroyOnLoad(go);
        go.AddComponent<AnimationInstaller>();
    }

    IEnumerator Start()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.5f);

        while (true)
        {
            Install<EnemyMelee>(EnemyModelAnimator.Kind.Melee);
            Install<EnemyRanged>(EnemyModelAnimator.Kind.Ranged);
            Install<EnemyBoss>(EnemyModelAnimator.Kind.Boss);

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                if (p.GetComponent<PlayerArmsAnimator>() == null) p.AddComponent<PlayerArmsAnimator>();
                if (p.GetComponent<PlayerBodyAnimator>() == null) p.AddComponent<PlayerBodyAnimator>();
            }

            yield return wait;
        }
    }

    static void Install<T>(EnemyModelAnimator.Kind kind) where T : MonoBehaviour
    {
        T[] all = FindObjectsByType<T>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].GetComponent<EnemyModelAnimator>() != null) continue;
            EnemyModelAnimator a = all[i].gameObject.AddComponent<EnemyModelAnimator>();
            a.Configure(kind);
        }
    }
}
