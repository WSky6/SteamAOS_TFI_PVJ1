using UnityEngine;
using StarterAssets;

// Sonido de pasos: caminar, correr y agachado.
// Los sonidos se cargan solos desde Assets/Resources/Audio/Footsteps/ (footstep_walk_1..3 y footstep_run_1..3).
// Si querés usar otros, arrastralos a las listas del Inspector y tienen prioridad.
[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Sonidos (opcional: si están vacíos se cargan de Resources)")]
    [SerializeField] AudioClip[] walkClips;
    [SerializeField] AudioClip[] runClips;

    [Header("Cada cuántos metros suena un paso")]
    [SerializeField] float walkStride = 1.8f;
    [SerializeField] float runStride = 2.3f;
    [SerializeField] float crouchStride = 1.4f;

    [Header("Volumen")]
    [SerializeField, Range(0f, 1f)] float walkVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] float runVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] float crouchVolumeMultiplier = 0.4f;

    CharacterController cc;
    FirstPersonController fpc;
    AudioSource source;
    float distance;
    int lastIndex = -1;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        fpc = GetComponent<FirstPersonController>();

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;   // sonido 2D: se escucha igual en cualquier lado

        if (walkClips == null || walkClips.Length == 0)
            walkClips = Filter("footstep_walk");
        if (runClips == null || runClips.Length == 0)
            runClips = Filter("footstep_run");
    }

    static AudioClip[] Filter(string prefix)
    {
        AudioClip[] all = Resources.LoadAll<AudioClip>("Audio/Footsteps");
        System.Collections.Generic.List<AudioClip> list = new System.Collections.Generic.List<AudioClip>();
        foreach (AudioClip c in all)
            if (c.name.StartsWith(prefix)) list.Add(c);
        return list.ToArray();
    }

    void Update()
    {
        // No suena en pausa, en el aire ni parado
        if (Time.timeScale == 0f) return;

        bool grounded = fpc != null ? fpc.Grounded : cc.isGrounded;
        Vector3 v = cc.velocity;
        v.y = 0f;
        float speed = v.magnitude;

        if (!grounded || speed < 0.5f)
        {
            // Parado: el próximo paso suena casi enseguida al empezar a moverse
            distance = Mathf.Max(distance, 0f);
            return;
        }

        bool crouching = PlayerCrouch.IsCrouching;
        bool running = !crouching && fpc != null &&
                       speed > (fpc.MoveSpeed + fpc.SprintSpeed) * 0.5f;

        float stride = crouching ? crouchStride : (running ? runStride : walkStride);

        distance += speed * Time.deltaTime;
        if (distance >= stride)
        {
            distance = 0f;
            PlayStep(running, crouching);
        }
    }

    void PlayStep(bool running, bool crouching)
    {
        AudioClip[] clips = running ? runClips : walkClips;
        if (clips == null || clips.Length == 0) return;

        // Elige uno distinto al anterior para que no se repita
        int i = Random.Range(0, clips.Length);
        if (clips.Length > 1 && i == lastIndex) i = (i + 1) % clips.Length;
        lastIndex = i;

        float volume = running ? runVolume : walkVolume;
        if (crouching) volume *= crouchVolumeMultiplier;

        source.pitch = Random.Range(0.93f, 1.07f);
        source.PlayOneShot(clips[i], volume * Random.Range(0.9f, 1f));
    }
}
